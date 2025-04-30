using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using GgStatAggregator.Data;
using GgStatAggregator.Result;
using GgStatAggregator.Helpers;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using GgStatAggregator.Models;

namespace GgStatAggregator.Services
{
    public abstract class BaseService<T>(GgStatAggregatorDbContext dbContext) 
        : IService<T> where T : EntityBase
    {
        protected readonly GgStatAggregatorDbContext _dbContext = dbContext;

        #region Public Methods
        public virtual async Task<Result<T?>> GetFirstOrDefaultAsync(Expression<Func<T?, bool>> predicate)
            => await GetFirstOrDefaultInternalAsync(predicate);

        public async Task<Result<T?>> GetMostRecentOrDefaultAsync(Expression<Func<T?, bool>> predicate) 
            => await GetFirstOrDefaultInternalAsync(
                predicate, 
                q => q.OrderByDescending(e => e!.CreatedAt));

        public virtual async Task<Result<List<T>>> GetAllAsync(Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> query = BuildQuery().Where(predicate ?? (_ => true));

            List<T> list = await query.ToListAsync();

            var message = list.Count == 0
                ? $"No {StringHelper.Pluralize(typeof(T).Name)} found"
                : null;

            return Result<List<T>>.Success(list, message);
        }

        public virtual async Task<Result<T?>> AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            return await ExecuteDbContextOperationAsync(() => _dbContext.SaveChangesAsync(), "adding", entity);
        }

        public virtual async Task<Result<T?>> UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            return await ExecuteDbContextOperationAsync(() => _dbContext.SaveChangesAsync(), "updating", entity);
        }

        public virtual async Task<Result<T?>> DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return await ExecuteDbContextOperationAsync(() => _dbContext.SaveChangesAsync(), "deleting", entity);
        }

        public virtual async Task<Result<T?>> StageAsync(T entity, StageAction action)
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
                    return Result<T?>.Failure($"Invalid action: {GetActionName(action)}");
            }

            return await ExecuteDbContextOperationAsync(() => Task.CompletedTask, GetActionName(action), entity);
        }

        public bool HasStagedChanges()
            => _dbContext.ChangeTracker
            .Entries<T>()
            .Any(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached);

        public Task CommitAsync() => _dbContext.SaveChangesAsync();

        public Task ClearStagedChanges()
        {
            _dbContext.ChangeTracker.Clear();
            return Task.CompletedTask;
        }
        #endregion Public Methods

        #region Protected Methods
        protected virtual IQueryable<T> BuildQuery() => _dbContext.Set<T>();
        #endregion Protected Methods

        #region Private Methods
        private async Task<Result<T?>> GetFirstOrDefaultInternalAsync(
            Expression<Func<T?, bool>> predicate,
            Func<IQueryable<T?>, IOrderedQueryable<T?>>? orderBy = null)
        {
            IQueryable<T?> query = BuildQuery().Where(predicate);

            if (orderBy != null)
                query = orderBy(query);


            T? model = await query.FirstOrDefaultAsync();

            if (model == null)
                return Result<T?>.Failure($"{typeof(T).Name} not found");

            return Result<T?>.Success(model);
        }

        private static async Task<Result<T?>> ExecuteDbContextOperationAsync(Func<Task> operation, string action, T entity)
        {
            try
            {
                await operation();
                return Result<T?>.Success(entity);
            }
            catch (DbUpdateException ex)
            {
                return Result<T?>.Failure($"Database update failed while {action} {typeof(T).Name}: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<T?>.Failure($"Unexpected error while {action} {typeof(T).Name}: {ex.Message}");
            }
        }

        private static string GetActionName(StageAction action) => action switch
        {
            StageAction.Add => "staging add",
            StageAction.Update => "staing update",
            StageAction.Delete => "staging delete",
            _ => "unknown action"
        };
        #endregion Private Methods
    }

    public enum StageAction
    {
        Add,
        Update,
        Delete
    }
}
