using System.ComponentModel.DataAnnotations;

namespace ModuleHelpDesk.Models
{
    public enum CategorieAction { HelpDesk, Developpement, Windows }

    public class Intervention
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public CategorieAction Categorie { get; set; }

        [Required]
        public double PrixForfaitaire { get; set; }

        [Required]
        public int DureeEstimeeMinutes { get; set; }
    }
}