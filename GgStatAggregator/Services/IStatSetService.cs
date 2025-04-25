using GgStatAggregator.Models;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public interface IStatSetService
    {
        Task<List<StatSet>> GetAllAsync();
        Task<List<StatSet>> GetAllAsync(Expression<Func<StatSet, bool>> filter = null);
        Task<StatSet> GetByIdAsync(int id);
        Task<StatSet> AddAsync(StatSet entity);
        Task<bool> UpdateAsync(StatSet entity);
        Task<bool> DeleteAsync(int id);
    }
}
