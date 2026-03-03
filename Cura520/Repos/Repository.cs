using Cura520.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Cura520.Repos
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;// = new ApplicationDbContext();
        private readonly DbSet<T> _dbSet;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
          //Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            )
        {
            //var entities = _dbSet.AsQueryable();
            IQueryable<T> query = _dbSet;
            //if (expression is not null)
            //{
            //    entities = entities.Where(expression);
            //}
            //if (includes is not null)
            //{
            //    foreach (var include in includes)
            //    {
            //        entities = entities.Include(include);
            //    }
            //}
            //if (!trackd)
            //{
            //    entities = entities.AsNoTracking();
            //}
            if (!tracked) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (expression != null) query = query.Where(expression);

            return await query.ToListAsync(cancellationToken);
            //return await entities.ToListAsync(cancellationToken);
        }
        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expressions = null,
          //Expression<Func<T, object>>?[] includes = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,

            bool tracked = true,
            CancellationToken cancellationToken = default
            )
        {
            IQueryable<T> query = _dbSet;
          //var entity = (await GetAsync(expressions, includes, trackd, cancellationToken)).FirstOrDefault();
            
            if (!tracked) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (expressions != null) query = query.Where(expressions);

          //return entity;
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
