using GgStatAggregator.Models;

namespace GgStatAggregator.Services
{
    public interface IStatSetService
    {
        Task<List<StatSet>> GetAllAsync();
        Task<StatSet> GetByIdAsync(int id);
        Task<StatSet> AddAsync(StatSet entity);
        Task<bool> UpdateAsync(StatSet entity);
        Task<bool> DeleteAsync(int id);
    }
}
