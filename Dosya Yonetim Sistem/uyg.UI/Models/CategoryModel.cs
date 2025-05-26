using System.ComponentModel.DataAnnotations;

namespace uyg.UI.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur")]
        public string Name { get; set; }

        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
} 