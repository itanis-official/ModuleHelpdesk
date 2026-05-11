using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuleHelpDesk.Models
{
    public enum StatutTicket 
    { 
        Nouveau,      // Fresh
        EnAttente,    // EnAttente
        Rejete,       // Rejected
        Ouvert,       // Open
        EnPause,      // Paused
        Clos,         // Closed
        Reserve       // Reserved
    }

    public enum PrioriteTicket 
    { 
        Basse, 
        Moyenne, 
        Haute, 
        Critique 
    }

    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        public int? InterventionId { get; set; }
        
        [ForeignKey("InterventionId")]
        public virtual Intervention? Intervention { get; set; }
        
        [Required]
        public CategorieAction Categorie { get; set; }

        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string ClientId { get; set; } = string.Empty;
        public string? SousClientId { get; set; }

        public StatutTicket Statut { get; set; } = StatutTicket.Nouveau;
        public PrioriteTicket Priorite { get; set; } = PrioriteTicket.Basse;

        // Retour à DateCreation en Français
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateFermeture { get; set; }
        
        public double DureeReelleMinutes { get; set; }
        public double CoutFinal { get; set; }

        public string? AgentPrincipalId { get; set; }
        public string? CodeUnidesk { get; set; }

        [Range(1, 5)]
        public int? Note { get; set; }
        public string? CommentaireAgent { get; set; }
        public string? CommentaireClient { get; set; }

        public List<string> ImagesUrls { get; set; } = new();

        [InverseProperty("Ticket")]
        public List<MessageTicket> Messages { get; set; } = new();
        public List<TicketCollaborateur> Collaborateurs { get; set; } = new();
    }
}