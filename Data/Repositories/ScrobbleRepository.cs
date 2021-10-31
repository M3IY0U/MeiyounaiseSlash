using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeiyounaiseSlash.Data.Models;

namespace MeiyounaiseSlash.Data.Repositories
{
    public class ScrobbleRepository : BaseRepository<Scrobble>
    {
        public ScrobbleRepository(MeiyounaiseContext ctx) : base(ctx)
        {
        }

        public virtual async Task ClearScrobblesForUserAsync(ulong userId)
        {
            var toClear = Entities.AsQueryable().Where(s => s.UserId == userId);
            Entities.RemoveRange(toClear);
            await Context.SaveChangesAsync();
        }

        public virtual async Task AddScrobblesAsync(IEnumerable<Scrobble> scrobbles)
            => await Entities.AddRangeAsync(scrobbles);

        public virtual async Task SaveChangesAsync() => await Context.SaveChangesAsync();
    }
}