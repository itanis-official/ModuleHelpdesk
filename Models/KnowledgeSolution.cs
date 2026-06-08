using System.ComponentModel.DataAnnotations;

namespace ModuleHelpDesk.Models
{
    public class KnowledgeSolution
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DescriptionResolution { get; set; } = string.Empty;

        [Required]
        public int AgentId { get; set; } 

        public DateTime DateResolution { get; set; } = DateTime.Now;

        public List<string> PiecesJointesUrls { get; set; } = new();

        public int KnowledgeBaseId { get; set; }
    }
}