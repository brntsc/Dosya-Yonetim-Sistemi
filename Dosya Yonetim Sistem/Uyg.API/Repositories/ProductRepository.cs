using Uyg.API.Data;
using Uyg.API.Models;

namespace Uyg.API.Repositories
{
    public class ProductRepository : GenericRepository<Product>
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }
    }
}
