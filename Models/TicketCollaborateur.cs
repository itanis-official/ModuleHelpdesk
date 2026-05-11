using System.ComponentModel.DataAnnotations;

namespace ModuleHelpDesk.Models
{
    public class TicketCollaborateur
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public required string AgentId { get; set; } 
    }
}