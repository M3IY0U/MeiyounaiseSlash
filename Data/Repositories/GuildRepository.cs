using System.Linq;
using System.Threading.Tasks;
using MeiyounaiseSlash.Data.Models;

namespace MeiyounaiseSlash.Data.Repositories
{
    public class GuildRepository : BaseRepository<Guild>
    {
        public GuildRepository(MeiyounaiseContext ctx) : base(ctx)
        {
        }

        public virtual async Task<Guild> GetOrCreateGuild(ulong id)
        {
            var guild = Entities.SingleOrDefault(g => g.Id == id);
            if (guild is not null)
            {
                return guild;
            }

            guild = new Guild
            {
                Id = id,
                JoinChannel = 0,
                LeaveChannel = 0,
                JoinMessage = string.Empty,
                LeaveMessage = string.Empty,
                RepeatMessages = 0
            };

            Entities.Add(guild);
            await Context.SaveChangesAsync();
            return guild;
        }

        public virtual async Task SetRepeatMsg(ulong id, long amount)
        {
            var guild = await GetOrCreateGuild(id);
            guild.RepeatMessages = (int) amount;
            Entities.Update(guild);
            await Context.SaveChangesAsync();
        }

        public virtual async Task SetChannel(ulong guildId, ulong channelId, bool isJoinChannel)
        {
            var guild = await GetOrCreateGuild(guildId);

            if (isJoinChannel)
                guild.JoinChannel = channelId;
            else
                guild.LeaveChannel = channelId;

            Entities.Update(guild);
            await Context.SaveChangesAsync();
        }

        public virtual async Task SetMessage(ulong guildId, string message, bool isJoinMessage)
        {
            var guild = await GetOrCreateGuild(guildId);

            if (isJoinMessage)
                guild.JoinMessage = message;
            else
                guild.LeaveMessage = message;

            Entities.Update(guild);
            await Context.SaveChangesAsync();
        }
    }
}