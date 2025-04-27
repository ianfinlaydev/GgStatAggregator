using GgStatAggregator.Data;
using GgStatAggregator.Models;
using GgStatAggregator.Result;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public class PlayerService(GgStatAggregatorDbContext dbContext) : BaseService<Player>(dbContext)
    {
        public override async Task<Result<Player>> GetFirstOrDefaultAsync(Expression<Func<Player, bool>> predicate)
        {
            Player player = await _dbContext.Players
                .Include(p => p.StatSets)
                .FirstOrDefaultAsync(predicate);

            if (player == null)
                return Result<Player>.Failure($"Player not found");

            return Result<Player>.Success(player);
        }

        public override async Task<Result<List<Player>>> GetAllAsync(Expression<Func<Player, bool>> predicate = null)
        {
            IQueryable<Player> query = _dbContext.Players.AsNoTracking()
                .Where(predicate ?? (_ => true));

            var players = await query
                .Include(p => p.StatSets)
                .ToListAsync();

            var message = players.Count == 0
                ? $"No players found"
                : null;

            return Result<List<Player>>.Success(players, message);
        }
    }
}
