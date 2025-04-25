using GgStatAggregator.Models;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public interface ITableService
    {
        Task<List<Table>> GetAllAsync();
        Task<List<Table>> GetAllAsync(Expression<Func<Table, bool>> filter);
        Task<Table> GetByIdAsync(int id);
        Task<Table> AddAsync(Table entity);
        Task<bool> UpdateAsync(Table entity);
        Task<bool> DeleteAsync(int id);
    }
}
