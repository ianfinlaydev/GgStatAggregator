using GgStatAggregator.Data;
using GgStatAggregator.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public class TableService(GgStatAggregatorDbContext dbContext) : BaseService<Table>(dbContext)
    {
    }
}
