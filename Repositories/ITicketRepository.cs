using ModuleHelpDesk.Models;

namespace ModuleHelpDesk.Repositories
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetByClientAsync(int clientId);
        Task<IEnumerable<Ticket>> GetBySubClientAsync(int subClientId);
        Task<IEnumerable<Ticket>> GetByAgentAsync(int agentId);

        Task<Ticket> CreateAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket);
        Task DeleteAsync(int id);
        Task ChangeStatusAsync(int ticketId, int newStatus); 
        Task<IEnumerable<Ticket>> GetByStatusAsync(StatutTicket status);
        Task<IEnumerable<Ticket>> GetByPriorityAsync(PrioriteTicket priority);

        Task<IEnumerable<Ticket>> GetByCollaborateurAsync(int agentId);



        Task TransferTicketAsync(int ticketId, int newAgentId);

        Task AddCollaborateursAsync(int ticketId, List<int> newAgentIds);
        
        Task SyncCollaborateursAsync(int ticketId, List<int> newAgentIds);

        Task<IEnumerable<TicketCollaborateur>> GetCollaborateursByTicketIdAsync(int ticketId);

        Task<IEnumerable<Ticket>> GetTicketsForFacturationAsync(int clientId, DateTime startDate, DateTime endDate);





        // Interventions
        Task<IEnumerable<Intervention>> GetAllInterventionsAsync();
        Task<Intervention> CreateInterventionAsync(Intervention intervention);
        Task<Intervention?> GetInterventionByIdAsync(int id);
        Task<IEnumerable<Intervention>> GetInterventionsByCategorieAsync(int categorie);
        Task UpdateInterventionAsync(Intervention intervention);
        Task DeleteInterventionAsync(int id);


        Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBaseAsync();
        Task<KnowledgeBase?> GetKnowledgeBaseByIdAsync(int id);
        Task<KnowledgeBase> CreateKnowledgeBaseAsync(KnowledgeBase kb);
        Task<IEnumerable<KnowledgeBase>> GetKnowledgeByCategorieAsync(int categorie);
        Task DeleteKnowledgeBaseAsync(int id);
        Task PatchKnowledgeBaseAsync(int id, KnowledgeBase updatedFields);

        Task<IEnumerable<KnowledgeSolution>> GetSolutionsByKbIdAsync(int kbId);
        Task<KnowledgeSolution> AddSolutionToKbAsync(KnowledgeSolution solution);
        Task DeleteSolutionAsync(int id);
        Task PatchSolutionAsync(int id, KnowledgeSolution updatedFields);

        Task<IEnumerable<Company>> GetAllCompaniesAsync();
Task<Company?> GetCompanyByIdAsync(int id);

Task<IEnumerable<Agent>> GetAllAgentsAsync();
Task<Agent?> GetAgentByIdAsync(int id);

Task<IEnumerable<Contact>> GetAllContactsAsync();
Task<IEnumerable<Contact>> GetContactsByCompanyAsync(int companyId);
Task<Agent?> GetDedicatedAgentByCompanyAsync(int companyId);
        Task<Agent?> GetAgentByEmailAsync(string email);
        

    }
}