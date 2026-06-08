using ITANIS.SharedEvents;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuleHelpDesk.Models;
using ModuleHelpDesk.Repositories;

namespace ModuleHelpDesk.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;

        public TicketsController(ITicketRepository repo, IPublishEndpoint publishEndpoint)
        {
            _repo = repo;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _repo.GetByIdAsync(id);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClient(int clientId) => Ok(await _repo.GetByClientAsync(clientId));

        [HttpGet("agent/{agentId}")]
        public async Task<IActionResult> GetByAgent(int agentId) => Ok(await _repo.GetByAgentAsync(agentId));

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromForm] string Titre,
            [FromForm] string Description,
            [FromForm] int Categorie,
            [FromForm] int ClientId,
            [FromForm] int? AgentPrincipalId,
            [FromForm] int? InterventionId,
            [FromForm] int Priorite,
            [FromForm] List<IFormFile> files)
        {
            var ticket = new Ticket
            {
                Titre = Titre,
                Description = Description,
                Categorie = (CategorieAction)Categorie,
                ClientId = ClientId,
                AgentPrincipalId = AgentPrincipalId,
                InterventionId = InterventionId,
                Priorite = (PrioriteTicket)Priorite,
                Statut = StatutTicket.Nouveau,
                DateCreation = DateTime.Now
            };

            var created = await _repo.CreateAsync(ticket);

            if (files != null && files.Count > 0)
            {
                var imageUrls = new List<string>();
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                foreach (var file in files)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    imageUrls.Add($"/uploads/tickets/{fileName}");
                }
                created.ImagesUrls = imageUrls;
                await _repo.UpdateAsync(created);
            }

            await PublishTicketSync(created, SyncAction.Created);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
public async Task<IActionResult> Update(int id, Ticket ticket)
{
    if (id != ticket.Id) return BadRequest();
    
    await _repo.UpdateAsync(ticket);

    await PublishTicketSync(ticket, SyncAction.Updated);

    if (ticket.LastSessionMetadata != null && ticket.LastSessionMetadata.MinutesWorked > 0)
    {
        await _publishEndpoint.Publish(new TimesheetSessionEvent
        {
            TicketId = ticket.Id,
            NomTache = $"Ticket #{ticket.Id} - {ticket.Titre}",
            Description = !string.IsNullOrEmpty(ticket.CommentaireAgent) 
                ? ticket.CommentaireAgent 
                : $"Intervention session on ticket: {ticket.Titre}",
            AgentId = ticket.LastSessionMetadata.AgentId,
            DateDebut = ticket.LastSessionMetadata.DateDebut,
            DateFin = ticket.LastSessionMetadata.DateFin,
            TotalHeures = ticket.LastSessionMetadata.MinutesWorked / 60.0, 
            Source = "HelpDesk"
        });
    }

    return NoContent();
}

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] int newStatus)
        {
            await _repo.ChangeStatusAsync(id, newStatus);

            // Fetch updated ticket to sync the new status
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket != null)
                await PublishTicketSync(ticket, SyncAction.Updated);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket != null)
                await PublishTicketSync(ticket, SyncAction.Deleted);

            await _repo.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("filter/status/{status}")]
        public async Task<IActionResult> GetByStatus(StatutTicket status) => Ok(await _repo.GetByStatusAsync(status));

        [HttpGet("filter/priority/{priority}")]
        public async Task<IActionResult> GetByPriority(PrioriteTicket priority) => Ok(await _repo.GetByPriorityAsync(priority));

        [HttpPut("{id}/transfer")]
        public async Task<IActionResult> TransferTicket(int id, [FromBody] int newAgentId)
        {
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket == null) return NotFound();

            await _repo.TransferTicketAsync(id, newAgentId);

            // Fetch again to get updated state
            var updated = await _repo.GetByIdAsync(id);
            if (updated != null)
                await PublishTicketSync(updated, SyncAction.Updated);

            return Ok(new { message = $"Ticket transféré à l'agent {newAgentId}" });
        }

        [HttpGet("collaborateur/{agentId}")]
        public async Task<IActionResult> GetByCollaborateur(int agentId)
        {
            try
            {
                var tickets = await _repo.GetByCollaborateurAsync(agentId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/collaborateurs")]
        public async Task<IActionResult> GetCollaborateurs(int id)
            => Ok(await _repo.GetCollaborateursByTicketIdAsync(id));

        [HttpPost("{id}/collaborateurs/bulk")]
        public async Task<IActionResult> AddCollaborateurs(int id, [FromBody] List<int> agentIds)
        {
            await _repo.AddCollaborateursAsync(id, agentIds);

            var collaborateurs = await _repo.GetCollaborateursByTicketIdAsync(id);
            foreach (var c in collaborateurs.Where(c => agentIds.Contains(c.AgentId)))
                await PublishCollaborateurSync(c, SyncAction.Created);

            return Ok(new { message = "Collaborateurs ajoutés." });
        }

        [HttpPut("{id}/collaborateurs/sync")]
        public async Task<IActionResult> SyncCollaborateurs(int id, [FromBody] List<int> agentIds)
        {
            // Get current list before sync to know what was removed
            var before = await _repo.GetCollaborateursByTicketIdAsync(id);

            await _repo.SyncCollaborateursAsync(id, agentIds);

            var after = await _repo.GetCollaborateursByTicketIdAsync(id);

            // Publish deleted ones
            var afterIds = after.Select(c => c.AgentId).ToHashSet();
            foreach (var removed in before.Where(c => !afterIds.Contains(c.AgentId)))
                await PublishCollaborateurSync(removed, SyncAction.Deleted);

            // Publish created ones
            var beforeIds = before.Select(c => c.AgentId).ToHashSet();
            foreach (var added in after.Where(c => !beforeIds.Contains(c.AgentId)))
                await PublishCollaborateurSync(added, SyncAction.Created);

            return Ok(new { message = "Liste des collaborateurs synchronisée." });
        }

        [HttpGet("facturation")]
        public async Task<IActionResult> GetFacture([FromQuery] int clientId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var tickets = await _repo.GetTicketsForFacturationAsync(clientId, start, end);
            return Ok(new
            {
                TotalTickets = tickets.Count(),
                Tickets = tickets,
                Periode = $"Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}"
            });
        }

        // ── Private helpers ───────────────────────────────────────────

        private Task PublishTicketSync(Ticket t, SyncAction action)
        {
            return _publishEndpoint.Publish(new TicketSyncEvent
            {
                Action = System.Text.Json.JsonSerializer.SerializeToElement((int)action),
                Id = t.Id,
                InterventionId = t.InterventionId,
                Categorie = t.Categorie.ToString(),
                Titre = t.Titre,
                Description = t.Description,
                ClientId = t.ClientId,
                SousClientId = t.SousClientId,
                Statut = t.Statut.ToString(),
                Priorite = t.Priorite.ToString(),
                DateCreation = t.DateCreation,
                DateFermeture = t.DateFermeture,
                DureeReelleMinutes = t.DureeReelleMinutes,
                CoutFinal = t.CoutFinal,
                AgentPrincipalId = t.AgentPrincipalId,
                CodeUnidesk = t.CodeUnidesk,
                ChangedAt = DateTime.UtcNow,
            });
        }

        private Task PublishCollaborateurSync(TicketCollaborateur tc, SyncAction action)
        {
            return _publishEndpoint.Publish(new TicketCollaborateurSyncEvent
            {
                Action = System.Text.Json.JsonSerializer.SerializeToElement((int)action),
                Id = tc.Id,
                TicketId = tc.TicketId,
                AgentId = tc.AgentId,
                ChangedAt = DateTime.UtcNow,
            });
        }
    }
}