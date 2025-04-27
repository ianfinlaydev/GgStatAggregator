using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using GgStatAggregator.Data;
using GgStatAggregator.Result;
using GgStatAggregator.Helpers;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GgStatAggregator.Services
{
    public abstract class BaseService<T>(GgStatAggregatorDbContext dbContext) : IService<T> where T : class
    {
        protected readonly GgStatAggregatorDbContext _dbContext = dbContext;

        public virtual async Task<Result<T>> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            IQueryable<T> query = _dbContext.Set<T>().Where(predicate);

            if (orderBy != null)
                query = orderBy(query);


            T model = await query.FirstOrDefaultAsync(predicate);

            if (model == null)
                return Result<T>.Failure($"{typeof(T).Name} not found");

            return Result<T>.Success(model);
        }

        public virtual async Task<Result<List<T>>> GetAllAsync(Expression<Func<T, bool>> predicate = null)
        {
            IQueryable<T> query = _dbContext.Set<T>().AsNoTracking()
                .Where(predicate ?? (_ => true));

            var list = await query.ToListAsync();

            var message = list.Count == 0
                ? $"No {StringHelper.Pluralize(typeof(T).Name)} found"
                : null;

            return Result<List<T>>.Success(list, message);
        }

        public virtual async Task<Result<T>> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            return await ExecuteDbContextOperationAsync(() => _dbContext.SaveChangesAsync(), "adding", entity);
        }

        public virtual async Task<Result<T>> UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            return await ExecuteDbContextOperationAsync(() => _dbContext.SaveChangesAsync(), "updating", entity);
        }

        public virtual async Task<Result<T>> DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return await ExecuteDbContextOperationAsync(() => _dbContext.SaveChangesAsync(), "deleting", entity);
        }

        public virtual async Task<Result<T>> StageAsync(T entity, StageAction action)
        {
            switch (action)
            {
                case StageAction.Add:
                    _dbContext.Set<T>().Add(entity);
                    break;
                case StageAction.Update:
                    _dbContext.Set<T>().Update(entity);
                    break;
                case StageAction.Delete:
                    _dbContext.Set<T>().Remove(entity);
                    break;
                default:
                    return Result<T>.Failure($"Invalid action: {GetActionName(action)}");
            }

            return await ExecuteDbContextOperationAsync(() => Task.CompletedTask, GetActionName(action), entity);
        }

        public bool HasStagedChanges()
        {
            return _dbContext.ChangeTracker
                .Entries<T>()
                .Any(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached);
        }

        public Task CommitAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

        public Task ClearStagedChanges()
        {
            _dbContext.ChangeTracker.Clear();
            return Task.CompletedTask;
        }

        private static async Task<Result<T>> ExecuteDbContextOperationAsync(Func<Task> operaiton, string action, T entity)
        {
            try
            {
                await operaiton();
                return Result<T>.Success(entity);
            }
            catch (DbUpdateException ex)
            {
                return Result<T>.Failure($"Database update failed while {action} {typeof(T).Name}: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<T>.Failure($"Unexpected error while {action} {typeof(T).Name}: {ex.Message}");
            }
        }

        private static string GetActionName(StageAction action) => action switch
        {
            StageAction.Add => "staging add",
            StageAction.Update => "staing update",
            StageAction.Delete => "staging delete",
            _ => "unknown action"
        };
    }

    public enum StageAction
    {
        Add,
        Update,
        Delete
    }
}
