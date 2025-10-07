using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace OpenAiChat.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        // Expose dbSet for query
        DbSet<T> GetDbSet();

        // READ operations
        Task<IEnumerable<T>> GetAllAsync();
        IQueryable<T> Find(Expression<Func<T, bool>> expression);

        // WRITE operations
        void Add(T entity);
    }
}
