using GgStatAggregator.Models;

namespace GgStatAggregator.Services
{
    public interface ITableService
    {
        Task<List<Table>> GetAllAsync();
        Task<Table> GetByIdAsync(int id);
        Task<Table> GetByStakeAndNumber(Stake stake, int number);
        Task<Table> AddAsync(Table entity);
        Task<bool> UpdateAsync(Table entity);
        Task<bool> DeleteAsync(int id);
    }
}
