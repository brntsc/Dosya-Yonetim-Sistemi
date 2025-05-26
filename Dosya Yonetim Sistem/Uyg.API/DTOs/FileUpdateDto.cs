using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Uyg.API.DTOs
{
    public class FileUpdateDto
    {
        public IFormFile? File { get; set; }
        
        [Required(ErrorMessage = "Dosya adı zorunludur")]
        [StringLength(255, ErrorMessage = "Dosya adı en fazla 255 karakter olabilir")]
        public string FileName { get; set; }
        
        [Required(ErrorMessage = "Açıklama zorunludur")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçilmelidir")]
        public int CategoryId { get; set; }

        public int[]? TagIds { get; set; }
    }
} 