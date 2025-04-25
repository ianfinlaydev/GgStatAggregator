using GgStatAggregator.Data;
using GgStatAggregator.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GgStatAggregator.Services
{
    public class TableService : ITableService
    {
        protected readonly GgStatAggregatorDbContext _context;

        public TableService(GgStatAggregatorDbContext context)
        {
            _context = context;
        }

        public async Task<List<Table>> GetAllAsync() => await _context.Tables.ToListAsync();

        public async Task<List<Table>> GetAllAsync(Expression<Func<Table, bool>> filter)
            => await _context.Tables.AsQueryable().Where(filter).ToListAsync();

        public async Task<Table> GetByIdAsync(int id) => await _context.Tables.FindAsync(id);

        public async Task<Table> AddAsync(Table table)
        {
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
            return table;
        }

        public async Task<bool> UpdateAsync(Table table)
        {
            var existingTable = await _context.Tables.FindAsync(table.Id);
            if (existingTable == null)
            {
                return false;
            }

            existingTable.Stake = table.Stake;
            existingTable.TableNumber = table.TableNumber;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingTable = await _context.Tables.FindAsync(id);
            if (existingTable == null)
            {
                return false;
            }

            _context.Tables.Remove(existingTable);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
