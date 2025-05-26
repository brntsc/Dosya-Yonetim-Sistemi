namespace Uyg.API.Models
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string UserId { get; set; }
        public string PhotoUrl { get; set; }
        public Category? Category { get; set; }  
        public AppUser? User { get; set; }
    }
}
