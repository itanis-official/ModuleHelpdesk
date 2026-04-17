using Microsoft.EntityFrameworkCore;
using ModuleHelpDesk.Data;
using ModuleHelpDesk.Models;

namespace ModuleHelpDesk.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly HelpDeskDbContext _context;
        public TicketRepository(HelpDeskDbContext context) => _context = context;

        #region Tickets Logic

        // On ajoute .Include partout pour avoir les détails de l'intervention
        public async Task<IEnumerable<Ticket>> GetAllAsync() 
            => await _context.Tickets.Include(t => t.Intervention).ToListAsync();

        public async Task<Ticket?> GetByIdAsync(int id) 
            => await _context.Tickets.Include(t => t.Intervention).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Ticket>> GetByClientAsync(string clientId) 
            => await _context.Tickets.Include(t => t.Intervention)
                .Where(t => t.ClientId == clientId).ToListAsync();

        public async Task<IEnumerable<Ticket>> GetBySubClientAsync(string subClientId) 
            => await _context.Tickets.Include(t => t.Intervention)
                .Where(t => t.SousClientId == subClientId).ToListAsync();

        public async Task<IEnumerable<Ticket>> GetByAgentAsync(string agentId) 
            => await _context.Tickets.Include(t => t.Intervention)
                .Where(t => t.AgentPrincipalId == agentId).ToListAsync();

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            ticket.DateCreation = DateTime.Now;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            _context.Entry(ticket).State = EntityState.Modified;
            // On empêche de modifier la date de création par erreur
            _context.Entry(ticket).Property(x => x.DateCreation).IsModified = false;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ChangeStatusAsync(int ticketId, int newStatus)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.Statut = (StatutTicket)newStatus; 
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Ticket>> GetByStatusAsync(StatutTicket status) 
            => await _context.Tickets.Include(t => t.Intervention)
                .Where(t => t.Statut == status).ToListAsync();

        public async Task<IEnumerable<Ticket>> GetByPriorityAsync(PrioriteTicket priority) 
            => await _context.Tickets.Include(t => t.Intervention)
                .Where(t => t.Priorite == priority).ToListAsync();

        public async Task<IEnumerable<Ticket>> GetTicketsForFacturationAsync(string clientId, DateTime startDate, DateTime endDate)
        {
            return await _context.Tickets
                .Include(t => t.Intervention)
                .Where(t => t.ClientId == clientId && 
                            t.Statut == StatutTicket.Clos && 
                            t.DateCreation >= startDate && 
                            t.DateCreation <= endDate)
                .ToListAsync();
        }

        #endregion

        #region Interventions Logic

        public async Task<IEnumerable<Intervention>> GetAllInterventionsAsync() 
            => await _context.Interventions.ToListAsync();

        public async Task<Intervention> CreateInterventionAsync(Intervention intervention)
        {
            _context.Interventions.Add(intervention);
            await _context.SaveChangesAsync();
            return intervention;
        }

        public async Task<Intervention?> GetInterventionByIdAsync(int id) 
            => await _context.Interventions.FindAsync(id);

        public async Task<IEnumerable<Intervention>> GetInterventionsByCategorieAsync(int categorie) 
            => await _context.Interventions
                .Where(i => (int)i.Categorie == categorie)
                .ToListAsync();

        public async Task UpdateInterventionAsync(Intervention intervention)
        {
            _context.Entry(intervention).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteInterventionAsync(int id)
        {
            var intervention = await _context.Interventions.FindAsync(id);
            if (intervention != null)
            {
                _context.Interventions.Remove(intervention);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Knowledge Logic

        public async Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBaseAsync() 
            => await _context.KnowledgeBases.Include(kb => kb.Solutions).ToListAsync();

        public async Task<KnowledgeBase?> GetKnowledgeBaseByIdAsync(int id) 
            => await _context.KnowledgeBases.Include(kb => kb.Solutions).FirstOrDefaultAsync(kb => kb.Id == id);

        public async Task<KnowledgeBase> CreateKnowledgeBaseAsync(KnowledgeBase kb)
        {
            kb.DateCreation = DateTime.Now;
            _context.KnowledgeBases.Add(kb);
            await _context.SaveChangesAsync();
            return kb;
        }

        public async Task<IEnumerable<KnowledgeBase>> GetKnowledgeByCategorieAsync(int categorie)
        {
            return await _context.KnowledgeBases
                .Include(kb => kb.Solutions)
                .Where(kb => (int)kb.Categorie == categorie)
                .ToListAsync();
        }

        public async Task<IEnumerable<KnowledgeSolution>> GetSolutionsByKbIdAsync(int kbId) 
            => await _context.KnowledgeSolutions.Where(s => s.KnowledgeBaseId == kbId).ToListAsync();

        public async Task<KnowledgeSolution> AddSolutionToKbAsync(KnowledgeSolution solution)
        {
            solution.DateResolution = DateTime.Now;
            _context.KnowledgeSolutions.Add(solution);
            await _context.SaveChangesAsync();
            return solution;
        }

        public async Task DeleteKnowledgeBaseAsync(int id)
        {
            var kb = await _context.KnowledgeBases.FindAsync(id);
            if (kb != null)
            {
                _context.KnowledgeBases.Remove(kb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task PatchKnowledgeBaseAsync(int id, KnowledgeBase updatedFields)
        {
            var existingKb = await _context.KnowledgeBases.FindAsync(id);
            if (existingKb == null) return;

            if (!string.IsNullOrWhiteSpace(updatedFields.NomErreur) && updatedFields.NomErreur != "string")
                existingKb.NomErreur = updatedFields.NomErreur;

            if (!string.IsNullOrWhiteSpace(updatedFields.DescriptionErreur) && updatedFields.DescriptionErreur != "string")
                existingKb.DescriptionErreur = updatedFields.DescriptionErreur;

            if (updatedFields.Categorie != 0) 
                existingKb.Categorie = updatedFields.Categorie;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteSolutionAsync(int id)
        {
            var solution = await _context.KnowledgeSolutions.FindAsync(id);
            if (solution != null)
            {
                _context.KnowledgeSolutions.Remove(solution);
                await _context.SaveChangesAsync();
            }
        }

        public async Task PatchSolutionAsync(int id, KnowledgeSolution updatedFields)
        {
            var existingSol = await _context.KnowledgeSolutions.FindAsync(id);
            if (existingSol == null) return;

            if (!string.IsNullOrWhiteSpace(updatedFields.DescriptionResolution) && updatedFields.DescriptionResolution != "string")
                existingSol.DescriptionResolution = updatedFields.DescriptionResolution;

            if (!string.IsNullOrWhiteSpace(updatedFields.AgentId) && updatedFields.AgentId != "string")
                existingSol.AgentId = updatedFields.AgentId;

            if (updatedFields.PiecesJointesUrls != null && updatedFields.PiecesJointesUrls.Any())
                existingSol.PiecesJointesUrls = updatedFields.PiecesJointesUrls;

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}