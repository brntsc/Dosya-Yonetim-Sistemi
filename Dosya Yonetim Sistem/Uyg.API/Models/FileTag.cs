using System.ComponentModel.DataAnnotations;

namespace Uyg.API.Models
{
    public class FileTag
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TagName { get; set; }
        
        public string Description { get; set; }
        
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual ICollection<FileModel> Files { get; set; } = new List<FileModel>();
    }
} 