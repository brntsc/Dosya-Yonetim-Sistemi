using Uyg.API.Data;
using Uyg.API.Models;

namespace Uyg.API.Repositories
{
    public class CategoryRepository : GenericRepository<Category>
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
