using System.ComponentModel.DataAnnotations;

namespace Uyg.API.Models
{
    public class Category : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual ICollection<FileModel> Files { get; set; }
    }
}
