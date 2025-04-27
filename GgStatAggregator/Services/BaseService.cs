using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using GgStatAggregator.Data;
using GgStatAggregator.Result;
using GgStatAggregator.Helpers;

namespace GgStatAggregator.Services
{
    public abstract class BaseService<T>(GgStatAggregatorDbContext dbContext) : IService<T> where T : class
    {
        protected readonly GgStatAggregatorDbContext _dbContext = dbContext;

        public virtual async Task<Result<T>> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            T model = await _dbContext.Set<T>()
                .FirstOrDefaultAsync(predicate);

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

        public virtual async Task<Result<T>> StageAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            return await ExecuteDbContextOperationAsync(() => Task.CompletedTask, "staging", entity);
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
    }
}
