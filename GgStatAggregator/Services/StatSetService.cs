using GgStatAggregator.Data;
using GgStatAggregator.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public class StatSetService(GgStatAggregatorDbContext dbContext) : BaseService<StatSet>(dbContext)
    {
    }
}
