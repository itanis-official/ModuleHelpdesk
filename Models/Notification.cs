using System.ComponentModel.DataAnnotations;

namespace ModuleHelpDesk.Models
{
    /// <summary>
    /// Type of receiver — determines which table to look up the receiver in.
    /// </summary>
    public enum ReceiverType
    {
        Agent,    // AgentPrincipalId or CollaborateurId
        Company,  // ClientId (Company)
        Contact   // SousClientId (Contact)
    }

    /// <summary>
    /// In-app notification stored in the Helpdesk DB.
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// The DB id of the receiver (Agent.Id, Company.Id, or Contact.Id).
        /// </summary>
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        public ReceiverType ReceiverType { get; set; }

        /// <summary>
        /// The ticket this notification relates to (nullable for system-level notifications).
        /// </summary>
        public int? TicketId { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
