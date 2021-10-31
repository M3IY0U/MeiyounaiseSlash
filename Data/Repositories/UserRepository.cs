using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeiyounaiseSlash.Data.Models;

namespace MeiyounaiseSlash.Data.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(MeiyounaiseContext ctx) : base(ctx)
        {
        }

        public virtual Task<bool> TryGetLast(ulong user, out string last)
        {
            last = Entities.SingleOrDefault(x => x.Id == user)?.LastFm;
            return Task.FromResult(!string.IsNullOrEmpty(last));
        }

        public virtual async Task SetLastAccount(ulong id, string last)
        {
            var user = Entities.SingleOrDefault(x => x.Id == id);
            if (user is null)
            {
                Entities.Add(new User {Id = id, LastFm = last});
            }
            else
            {
                user.LastFm = last;
                Entities.Update(user);
            }

            await Context.SaveChangesAsync();
        }

        public virtual async Task AddReaction(ulong userId, string reaction)
        {
            var user = Entities.SingleOrDefault(u => u.Id == userId);
            if (user is null) return;
            user.NowPlayingReactions ??= new HashSet<string>();
            user.NowPlayingReactions.Add(reaction);
            Entities.Update(user);
            await Context.SaveChangesAsync();
        }
        
        public virtual async Task ClearReactions(ulong userId)
        {
            var user = Entities.SingleOrDefault(u => u.Id == userId);
            if (user is null) return;
            user.NowPlayingReactions ??= new HashSet<string>();
            user.NowPlayingReactions.Clear();
            Entities.Update(user);
            await Context.SaveChangesAsync();
        }
        
        public virtual async Task<bool> RemoveReactions(ulong userId, string reaction)
        {
            var user = Entities.SingleOrDefault(u => u.Id == userId);
            if (user is null) return false;
            user.NowPlayingReactions ??= new HashSet<string>();
            var result = user.NowPlayingReactions.Remove(reaction);
            Entities.Update(user);
            await Context.SaveChangesAsync();
            return result;
        }

        public virtual async Task<IEnumerable<string>> GetReactions(ulong userId)
        {
            var user = Entities.SingleOrDefault(u => u.Id == userId);
            if (user is null) return null;
            if (user.NowPlayingReactions is not null) return user.NowPlayingReactions;
            user.NowPlayingReactions = new HashSet<string>();
            Entities.Update(user);
            await Context.SaveChangesAsync();
            return user.NowPlayingReactions;
        }
        
        public virtual async Task<IEnumerable<(ulong id, string last)>> GetLastUsersInCurrentGuild(IEnumerable<ulong> memberIds)
        {
            var result = new List<(ulong, string)>();
            foreach (var id in memberIds)
            {
                if(await TryGetLast(id, out var last))
                    result.Add((id, last));
            }

            return result;
        }
    }
}