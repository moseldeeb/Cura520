using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Cura520.Repos
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
          //Expression<Func<T, object>>[]? includes = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            );
        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expressions = null,
          //Expression<Func<T, object>>?[] includes = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            );

        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
