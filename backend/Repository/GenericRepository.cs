using Microsoft.EntityFrameworkCore;
using OpenAiChat.Data;
using System.Linq.Expressions;

namespace OpenAiChat.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> 
        where T : class
    {
        protected readonly FileUploadEfDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(FileUploadEfDbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }
        public DbSet<T> GetDbSet()
        {
            return _dbSet;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var results = await _dbSet.ToListAsync().ConfigureAwait(false);
            return results;
        }
        public IQueryable<T> Find(Expression<Func<T, bool>> expression)
        {
            // Executes the query only when ToListAsync is called
            return _dbSet.Where(expression);
        }

        // WRITE Implementations (Note: these do NOT call SaveChanges; they just track the change)
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }
    }
}
