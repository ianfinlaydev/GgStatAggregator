using GgStatAggregator.Models;
using GgStatAggregator.Result;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public interface IService<T> where T : EntityBase
    {
        Task<Result<T?>> GetFirstOrDefaultAsync(Expression<Func<T?, bool>> predicate);
        Task<Result<T?>> GetMostRecentOrDefaultAsync(Expression<Func<T?, bool>> predicate);
        Task<Result<List<T>>> GetAllAsync(Expression<Func<T, bool>>? predicate = null);
        Task<Result<T?>> AddAsync(T entity);
        Task<Result<T?>> UpdateAsync(T entity);
        Task<Result<T?>> DeleteAsync(T entity);
        Task<Result<T?>> StageAsync(T entity, StageAction action);
        bool HasStagedChanges();
        Task CommitAsync();
        Task ClearStagedChanges();
    }
}
