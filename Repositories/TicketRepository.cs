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

        public async Task<IEnumerable<Ticket>> GetAllAsync() 
            => await _context.Tickets.Include(t => t.Intervention).ToListAsync();

        public async Task<Ticket?> GetByIdAsync(int id) 
            => await _context.Tickets.Include(t => t.Intervention).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Ticket>> GetByClientAsync(int clientId) 
            => await _context.Tickets.Include(t => t.Intervention)
                .Where(t => t.ClientId == clientId).ToListAsync();

        public async Task<IEnumerable<Ticket>> GetBySubClientAsync(int subClientId) 
            => await _context.Tickets.Include(t => t.Intervention)
                .Where(t => t.SousClientId == subClientId).ToListAsync();

        public async Task<IEnumerable<Ticket>> GetByCollaborateurAsync(int agentId)
        {
            return await _context.Tickets
                .Include(t => t.Collaborateurs) 
                .Where(t => t.Collaborateurs.Any(c => c.AgentId == agentId)) 
                .ToListAsync();
        }

public async Task<IEnumerable<Ticket>> GetByAgentAsync(int agentId) 
{
    return await _context.Tickets
        .Include(t => t.Intervention)
        .Include(t => t.Collaborateurs) 
        .Where(t => t.AgentPrincipalId == agentId || 
                    t.Collaborateurs.Any(c => c.AgentId == agentId)) 
        .ToListAsync();
}

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

        public async Task<IEnumerable<Ticket>> GetTicketsForFacturationAsync(int clientId, DateTime startDate, DateTime endDate)
        {
            return await _context.Tickets
                .Include(t => t.Intervention)
                .Where(t => t.ClientId == clientId && 
                            t.Statut == StatutTicket.Clos && 
                            t.DateCreation >= startDate && 
                            t.DateCreation <= endDate)
                .ToListAsync();
        }


public async Task TransferTicketAsync(int ticketId, int newAgentId)
{
    var ticket = await _context.Tickets.FindAsync(ticketId);
    if (ticket != null)
    {
        ticket.AgentPrincipalId = newAgentId;
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }
}

public async Task AddCollaborateursAsync(int ticketId, List<int> agentIds)

{
    var existingAgentIds = await _context.TicketCollaborateurs
        .Where(c => c.TicketId == ticketId)
        .Select(c => c.AgentId)
        .ToListAsync();
    var newCollabs = agentIds
        .Where(id => !existingAgentIds.Contains(id))
        .Select(id => new TicketCollaborateur 
        { 
            TicketId = ticketId, 
            AgentId = id 
        }).ToList();

    if (newCollabs.Any())
    {
        await _context.TicketCollaborateurs.AddRangeAsync(newCollabs);
        await _context.SaveChangesAsync();
    }
}

public async Task SyncCollaborateursAsync(int ticketId, List<int> newAgentIds)
{
    var currentCollabs = await _context.TicketCollaborateurs
        .Where(c => c.TicketId == ticketId)
        .ToListAsync();
    var toRemove = currentCollabs.Where(c => !newAgentIds.Contains(c.AgentId)).ToList();
    if (toRemove.Any())
    {
        _context.TicketCollaborateurs.RemoveRange(toRemove);
    }
    var currentIds = currentCollabs.Select(c => c.AgentId).ToList();
    var toAdd = newAgentIds
        .Where(id => !currentIds.Contains(id))
        .Select(id => new TicketCollaborateur { TicketId = ticketId, AgentId = id })
        .ToList();

    if (toAdd.Any())
    {
        await _context.TicketCollaborateurs.AddRangeAsync(toAdd);
    }
    await _context.SaveChangesAsync();
}

public async Task<IEnumerable<TicketCollaborateur>> GetCollaborateursByTicketIdAsync(int ticketId)
{
    return await _context.TicketCollaborateurs
        .Where(c => c.TicketId == ticketId)
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
            => await _context.KnowledgeBases
                .Include(kb => kb.Solutions)
                .Include(kb => kb.CreatedByAgent) 
                .ToListAsync();

        public async Task<KnowledgeBase?> GetKnowledgeBaseByIdAsync(int id) 
            => await _context.KnowledgeBases
                .Include(kb => kb.Solutions)
                .Include(kb => kb.CreatedByAgent) 
                .FirstOrDefaultAsync(kb => kb.Id == id);

        public async Task<KnowledgeBase> CreateKnowledgeBaseAsync(KnowledgeBase kb)
{
    _context.KnowledgeBases.Add(kb);
    await _context.SaveChangesAsync();

    return await _context.KnowledgeBases
        .Include(k => k.Solutions)
        .Include(k => k.CreatedByAgent) 
        .FirstOrDefaultAsync(k => k.Id == kb.Id);
}

        public async Task<IEnumerable<KnowledgeBase>> GetKnowledgeByCategorieAsync(int categorie)
        {
            return await _context.KnowledgeBases
                .Include(kb => kb.Solutions)
                .Include(kb => kb.CreatedByAgent) 
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

            // Update AgentId if a valid one is supplied
            if (updatedFields.AgentId != 0)
                existingKb.AgentId = updatedFields.AgentId;

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

            if (updatedFields.AgentId != 0)
                existingSol.AgentId = updatedFields.AgentId;

            if (updatedFields.PiecesJointesUrls != null && updatedFields.PiecesJointesUrls.Any())
                existingSol.PiecesJointesUrls = updatedFields.PiecesJointesUrls;

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Companies Logic

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
            => await _context.Companies.ToListAsync();

        public async Task<Company?> GetCompanyByIdAsync(int id)
            => await _context.Companies.FindAsync(id);

        #endregion

        #region Agents Logic

        public async Task<IEnumerable<Agent>> GetAllAgentsAsync()
            => await _context.Agents.ToListAsync();

        public async Task<Agent?> GetAgentByIdAsync(int id)
            => await _context.Agents.FindAsync(id);

        public async Task<Agent?> GetDedicatedAgentByCompanyAsync(int companyId)
{
    var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
    if (company == null || company.AgentResponsableId == null) return null;

    return await _context.Agents.FirstOrDefaultAsync(a => a.Id == company.AgentResponsableId);
}

        public async Task<Agent?> GetAgentByEmailAsync(string email)
            => await _context.Agents
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());

        #endregion

        #region Contacts Logic

        public async Task<IEnumerable<Contact>> GetAllContactsAsync()
            => await _context.Contacts.ToListAsync();

        public async Task<IEnumerable<Contact>> GetContactsByCompanyAsync(int companyId)
            => await _context.Contacts
                .Where(c => c.CompanyId == companyId)
                .ToListAsync();

        #endregion
    }
}