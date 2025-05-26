using Microsoft.EntityFrameworkCore;
using Uyg.API.Data;
using Uyg.API.Models;
using System.Threading.Tasks;

namespace Uyg.API.Repositories
{
    public class FileRepository : GenericRepository<FileModel>, IFileRepository
    {
        private readonly AppDbContext _context;

        public FileRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<FileTag> GetFileTagById(int id)
        {
            return await _context.FileTags.FindAsync(id);
        }

        public AppDbContext GetDbContext()
        {
            return _context;
        }

        // Özel metodlar buraya eklenebilir
    }
} 