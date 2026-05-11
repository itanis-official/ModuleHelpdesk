using System.ComponentModel.DataAnnotations;

namespace ModuleHelpDesk.Models
{

    
    public class KnowledgeBase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string NomErreur { get; set; } = string.Empty;

        [Required]
        public string DescriptionErreur { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; } = DateTime.Now;
        

         [Required]
        public CategorieAction Categorie { get; set; }


        public List<KnowledgeSolution> Solutions { get; set; } = new();
    }
}