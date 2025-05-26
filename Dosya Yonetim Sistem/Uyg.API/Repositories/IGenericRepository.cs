using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Uyg.API.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(object id);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(object id);
        IQueryable<T> Where(Expression<Func<T, bool>> predicate);
        IQueryable<T> Table { get; }
        Task Save();
    }
} 