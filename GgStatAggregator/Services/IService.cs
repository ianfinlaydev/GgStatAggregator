using GgStatAggregator.Result;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public interface IService<T> where T : class
    {
        Task<Result<T>> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);
        Task<Result<List<T>>> GetAllAsync(Expression<Func<T, bool>> predicate = null);
        Task<Result<T>> AddAsync(T entity);
        Task<Result<T>> UpdateAsync(T entity);
        Task<Result<T>> DeleteAsync(T entity);
        Task<Result<T>> StageAsync(T entity, StageAction action);
        bool HasStagedChanges();
        Task CommitAsync();
        Task ClearStagedChanges();
    }
}
