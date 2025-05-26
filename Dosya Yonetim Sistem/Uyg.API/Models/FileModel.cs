using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uyg.API.Models
{
    public class FileModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        public DateTime UploadDate { get; set; }
        
        public string UploadedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual ICollection<FileTag> FileTags { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual Category Category { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
} 