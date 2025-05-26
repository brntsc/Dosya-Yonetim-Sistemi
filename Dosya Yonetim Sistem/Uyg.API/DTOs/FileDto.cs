using System.ComponentModel.DataAnnotations;

namespace Uyg.API.DTOs
{
    public class FileDto
    {
        public int Id { get; set; }
        
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        public string Description { get; set; }
        
        public DateTime UploadDate { get; set; }
        
        public string UploadedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        
        public List<string> Tags { get; set; }
        public int CommentCount { get; set; }
        
        public string UserId { get; set; }
        public string UserName { get; set; }
        
        public List<int> FileTags { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public CategoryDto Category { get; set; }
        public UserDto User { get; set; }
    }
} 