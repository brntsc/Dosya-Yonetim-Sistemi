using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Uyg.API.DTOs
{
    public class FileUploadDto
    {
        [Required(ErrorMessage = "Dosya seçilmedi")]
        public IFormFile File { get; set; }
        
        public string FileName { get; set; }
        
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçilmelidir")]
        public int CategoryId { get; set; }
        
        public List<int> TagIds { get; set; } = new List<int>();
    }
} 