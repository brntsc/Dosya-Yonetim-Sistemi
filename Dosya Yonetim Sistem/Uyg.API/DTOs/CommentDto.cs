using System.ComponentModel.DataAnnotations;

namespace Uyg.API.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public string CreatedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        public int FileId { get; set; }
    }
} 