using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeiyounaiseSlash.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MeiyounaiseSlash.Data.Repositories
{
    public class ScrobbleRepository
    {
        private readonly MeiyounaiseContext _context;
        private readonly DbSet<Scrobble> _entities;

        public ScrobbleRepository(MeiyounaiseContext ctx)
        {
            _context = ctx;
            _entities = ctx.Set<Scrobble>();
        }
        
        public virtual async Task ClearScrobblesForUserAsync(ulong userId)
        {
            var toClear = _entities.AsQueryable().Where(s => s.UserId == userId);
            _entities.RemoveRange(toClear);
            await _context.SaveChangesAsync();
        }

        public virtual async Task AddScrobblesAsync(IEnumerable<Scrobble> scrobbles)
            => await _entities.AddRangeAsync(scrobbles);

        public virtual async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}