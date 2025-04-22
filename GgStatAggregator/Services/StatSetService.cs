using GgStatAggregator.Data;
using GgStatAggregator.Models;
using Microsoft.EntityFrameworkCore;

namespace GgStatAggregator.Services
{
    public class StatSetService : IStatSetService
    {
        protected readonly GgStatAggregatorDbContext _context;

        public StatSetService(GgStatAggregatorDbContext context)
        {
            _context = context;
        }

        public async Task<List<StatSet>> GetAllAsync() => await _context.StatSets.ToListAsync();

        public async Task<StatSet> GetByIdAsync(int id) => await _context.StatSets.FindAsync(id);

        public async Task<StatSet> AddAsync(StatSet statSet)
        {
            _context.StatSets.Add(statSet);
            await _context.SaveChangesAsync();
            return statSet;
        }

        public async Task<bool> UpdateAsync(StatSet statSet)
        {
            var existingStatSet = await _context.StatSets.FindAsync(statSet.Id);
            if (existingStatSet == null)
            {
                return false;
            }

            existingStatSet.PlayerId = statSet.PlayerId;
            existingStatSet.TableId = statSet.TableId;
            existingStatSet.Hands = statSet.Hands;
            existingStatSet.Vpip = statSet.Vpip;
            existingStatSet.Pfr = statSet.Pfr;
            existingStatSet.Steal = statSet.Steal;
            existingStatSet.ThreeBet = statSet.ThreeBet;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingStatSet = await _context.StatSets.FindAsync(id);
            if (existingStatSet == null)
            {
                return false;
            }

            _context.StatSets.Remove(existingStatSet);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
