using Uyg.API.Models;
using Uyg.API.Data;

namespace Uyg.API.Repositories
{
    public interface IFileRepository : IGenericRepository<FileModel>
    {
        Task<FileTag> GetFileTagById(int id);
        AppDbContext GetDbContext();
    }
} 