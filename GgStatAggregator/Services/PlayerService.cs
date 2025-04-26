using GgStatAggregator.Data;
using GgStatAggregator.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public class PlayerService(GgStatAggregatorDbContext dbContext) : BaseService<Player>(dbContext)
    {
        public override async Task<Player> GetFirstOrDefaultAsync(Expression<Func<Player, bool>> predicate) 
            => await _dbContext.Players.Include(p => p.StatSets).FirstOrDefaultAsync(predicate);

        public override async Task<List<Player>> GetAllAsync(Expression<Func<Player, bool>> predicate = null)
        {
            IQueryable<Player> query = _dbContext.Set<Player>().AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

            return await query.ToListAsync();
        }
    }
}
