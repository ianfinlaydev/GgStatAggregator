using GgStatAggregator.Models;

namespace GgStatAggregator.Services
{
    public interface IPlayerService
    {
        Task<List<Player>> GetAllAsync();
        Task<Player> GetByIdAsync(int id);
        Task<Player> AddAsync(Player entity);
        Task<bool> UpdateAsync(Player entity);
        Task<bool> DeleteAsync(int id);
    }
}
