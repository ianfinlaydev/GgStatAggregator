using GgStatAggregator.Models;

namespace GgStatAggregator.Services
{
    public interface ITableService
    {
        Task<List<Table>> GetAllAsync();
        Task<Table> GetByIdAsync(int id);
        Task<Table> AddAsync(Table entity);
        Task<bool> UpdateAsync(Table entity);
        Task<bool> DeleteAsync(int id);
    }
}
