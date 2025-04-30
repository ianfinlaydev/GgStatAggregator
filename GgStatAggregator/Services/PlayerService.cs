using GgStatAggregator.Data;
using GgStatAggregator.Models;
using GgStatAggregator.Result;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public class PlayerService(GgStatAggregatorDbContext dbContext) : BaseService<Player>(dbContext)
    {
        protected override IQueryable<Player> BuildQuery() => _dbContext.Players
            .Include(p => p.StatSets);
    }
}
