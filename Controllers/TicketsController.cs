using Microsoft.AspNetCore.Mvc;
using ModuleHelpDesk.Models;
using ModuleHelpDesk.Repositories;
using MassTransit; // Nécessaire pour la notification inter-modules

namespace ModuleHelpDesk.Controllers
{
    [ApiController]
    [Route("api/helpdesk/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;

        public TicketsController(ITicketRepository repo, IPublishEndpoint publishEndpoint)
        {
            _repo = repo;
            _publishEndpoint = publishEndpoint;
        }

        // --- Endpoints existants ---

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _repo.GetByIdAsync(id);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClient(string clientId) => Ok(await _repo.GetByClientAsync(clientId));

        [HttpGet("agent/{agentId}")]
        public async Task<IActionResult> GetByAgent(string agentId) => Ok(await _repo.GetByAgentAsync(agentId));

        [HttpPost]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            var created = await _repo.CreateAsync(ticket);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Ticket ticket)
        {
            if (id != ticket.Id) return BadRequest();
            await _repo.UpdateAsync(ticket);
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] int newStatus)
        {
            await _repo.ChangeStatusAsync(id, newStatus);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }

        // --- Filtres ---

        [HttpGet("filter/status/{status}")]
        public async Task<IActionResult> GetByStatus(StatutTicket status) => Ok(await _repo.GetByStatusAsync(status));

        [HttpGet("filter/priority/{priority}")]
        public async Task<IActionResult> GetByPriority(PrioriteTicket priority) => Ok(await _repo.GetByPriorityAsync(priority));

        // --- NOUVEAUX : Gestion des Agents et Transferts ---

        [HttpPut("{id}/transfer")]
        public async Task<IActionResult> TransferTicket(int id, [FromBody] string newAgentId)
        {
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket == null) return NotFound();

            await _repo.TransferTicketAsync(id, newAgentId);

            // Notification pour le module Timesheet (via RabbitMQ)
            await _publishEndpoint.Publish(new 
            {
                TicketId = id,
                NewAgentId = newAgentId,
                TransferredAt = DateTime.UtcNow
            });

            return Ok(new { message = $"Ticket transféré à l'agent {newAgentId}" });
        }

        // --- NOUVEAUX : Gestion des Collaborateurs ---

        [HttpGet("{id}/collaborateurs")]
        public async Task<IActionResult> GetCollaborateurs(int id) 
            => Ok(await _repo.GetCollaborateursByTicketIdAsync(id));

        [HttpPost("{id}/collaborateurs/bulk")]
        public async Task<IActionResult> AddCollaborateurs(int id, [FromBody] List<string> agentIds)
        {
            await _repo.AddCollaborateursAsync(id, agentIds);
            return Ok(new { message = "Collaborateurs ajoutés." });
        }

        [HttpPut("{id}/collaborateurs/sync")]
        public async Task<IActionResult> SyncCollaborateurs(int id, [FromBody] List<string> agentIds)
        {
            await _repo.SyncCollaborateursAsync(id, agentIds);
            return Ok(new { message = "Liste des collaborateurs synchronisée." });
        }

        // --- Facturation ---

        [HttpGet("facturation")]
        public async Task<IActionResult> GetFacture([FromQuery] string clientId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var tickets = await _repo.GetTicketsForFacturationAsync(clientId, start, end);
            return Ok(new
            {
                TotalTickets = tickets.Count(),
                Tickets = tickets,
                Periode = $"Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}"
            });
        }
    }
}