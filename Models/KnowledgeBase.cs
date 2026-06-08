using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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


        public int? AgentId { get; set; }
        [ForeignKey("AgentId")]
        public virtual Agent? CreatedByAgent { get; set; }
    }
}