using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ModuleHelpDesk.Models

{
    public class MessageTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; } 

    [ForeignKey("TicketId")] 
    public virtual Ticket Ticket { get; set; } = null!;

        [Required]

        public string EnvoyeParId { get; set; } = string.Empty; 

        [Required]

        public string Contenu { get; set; } = string.Empty;

        public DateTime DateEnvoi { get; set; } = DateTime.Now;

        public bool EstLu { get; set; } = false;

    }

}

