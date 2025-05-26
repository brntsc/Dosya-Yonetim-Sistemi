using System.ComponentModel.DataAnnotations;

namespace uyg.UI.Models
{
    public class ProductModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }
} 