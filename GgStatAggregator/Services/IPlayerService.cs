using GgStatAggregator.Models;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public interface IPlayerService
    {
        Task<List<Player>> GetAllAsync();
        Task<Player> GetByIdAsync(int id);
        Task<List<Player>> GetAllAsync(Expression<Func<Player, bool>> filter = null);
        Task<Player> AddAsync(Player entity);
        Task<bool> UpdateAsync(Player entity);
        Task<bool> DeleteAsync(int id);
    }
}
