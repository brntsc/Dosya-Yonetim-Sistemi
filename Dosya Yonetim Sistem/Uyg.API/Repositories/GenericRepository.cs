using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Uyg.API.Data;

namespace Uyg.API.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task Delete(object id)
        {
            try
            {
                var entity = await GetById(id);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                }
                else
                {
                    throw new KeyNotFoundException($"Entity with id {id} not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting entity with id {id}: {ex.Message}", ex);
            }
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public IQueryable<T> Table => _dbSet;

        public async Task Save()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("Concurrency error while saving changes", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Database update error while saving changes", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving changes", ex);
            }
        }
    }
}
