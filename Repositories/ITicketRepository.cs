using ModuleHelpDesk.Models;

namespace ModuleHelpDesk.Repositories
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket?> GetByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetByClientAsync(string clientId);
        Task<IEnumerable<Ticket>> GetBySubClientAsync(string subClientId);
        Task<IEnumerable<Ticket>> GetByAgentAsync(string agentId);

        Task<Ticket> CreateAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket);
        Task DeleteAsync(int id);
        Task ChangeStatusAsync(int ticketId, int newStatus); 
        Task<IEnumerable<Ticket>> GetByStatusAsync(StatutTicket status);
        Task<IEnumerable<Ticket>> GetByPriorityAsync(PrioriteTicket priority);

        // Facturation
        Task<IEnumerable<Ticket>> GetTicketsForFacturationAsync(string clientId, DateTime startDate, DateTime endDate);





        // Interventions
        Task<IEnumerable<Intervention>> GetAllInterventionsAsync();
        Task<Intervention> CreateInterventionAsync(Intervention intervention);
        Task<Intervention?> GetInterventionByIdAsync(int id);
        Task<IEnumerable<Intervention>> GetInterventionsByCategorieAsync(int categorie);
        Task UpdateInterventionAsync(Intervention intervention);
        Task DeleteInterventionAsync(int id);


        // Knowledge Base (Problèmes)
        Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBaseAsync();
        Task<KnowledgeBase?> GetKnowledgeBaseByIdAsync(int id);
        Task<KnowledgeBase> CreateKnowledgeBaseAsync(KnowledgeBase kb);
        Task<IEnumerable<KnowledgeBase>> GetKnowledgeByCategorieAsync(int categorie);
        Task DeleteKnowledgeBaseAsync(int id);
        Task PatchKnowledgeBaseAsync(int id, KnowledgeBase updatedFields); // Exemple de PATCH ciblé

        // Solutions
        Task<IEnumerable<KnowledgeSolution>> GetSolutionsByKbIdAsync(int kbId);
        Task<KnowledgeSolution> AddSolutionToKbAsync(KnowledgeSolution solution);
        Task DeleteSolutionAsync(int id);
        Task PatchSolutionAsync(int id, KnowledgeSolution updatedFields);
        

    }
}