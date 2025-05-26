using System.ComponentModel.DataAnnotations;

namespace Uyg.API.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public string CreatedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual FileModel File { get; set; }
        public int FileId { get; set; }
    }
} 