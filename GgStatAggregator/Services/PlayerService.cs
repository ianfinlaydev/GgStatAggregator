using GgStatAggregator.Data;
using GgStatAggregator.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public class PlayerService : IPlayerService
    {
        protected readonly GgStatAggregatorDbContext _context;

        public PlayerService(GgStatAggregatorDbContext context)
        {
            _context = context;
        }

        public async Task<List<Player>> GetAllAsync() 
            => await _context.Players.ToListAsync();

        public async Task<List<Player>> GetAllAsync(Expression<Func<Player, bool>> filter)
            => await _context.Players.AsQueryable().Where(filter).ToListAsync();

        public async Task<Player> GetByIdAsync(int id) => await _context.Players
            .Include(p => p.StatSets)
            .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Player> AddAsync(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task<bool> UpdateAsync(Player player)
        {
            var existingPlayer = await _context.Players.FindAsync(player.Id);
            if (existingPlayer == null)
            {
                return false;
            }

            existingPlayer.Name = player.Name;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingPlayer = await _context.Players.FindAsync(id);
            if (existingPlayer == null)
            {
                return false;
            }

            _context.Players.Remove(existingPlayer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
