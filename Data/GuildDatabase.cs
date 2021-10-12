using System;
using System.Linq.Expressions;
using LiteDB;

namespace MeiyounaiseSlash.Data
{
    public class GuildDatabase : BaseDatabase
    {
        #region POCOs

        public class Guild
        {
            public ulong Id { get; init; }
            public string JoinMessage { get; set; }
            public string LeaveMessage { get; set; }
            public ulong JoinChannel { get; set; }
            public ulong LeaveChannel { get; set; }
            public int RepeatMessages { get; set; }
        }

        #endregion


        public ILiteCollection<Guild> GuildCollection;

        public GuildDatabase(string path) : base(path)
        {
            GuildCollection = Database.GetCollection<Guild>("guilds");
        }

        public bool TryGetGuild(Expression<Func<Guild, bool>> predicate, out Guild guild)
        {
            guild = GuildCollection.FindOne(predicate);
            return guild is not null;
        }

        public Guild GetOrCreateGuild(ulong id)
        {
            if (TryGetGuild(g => g.Id == id, out var guild)) return guild;

            guild = new Guild
            {
                Id = id,
                JoinChannel = 0,
                LeaveChannel = 0,
                JoinMessage = string.Empty,
                LeaveMessage = string.Empty,
                RepeatMessages = 0
            };

            GuildCollection.Insert(guild);

            return guild;
        }

        public void SetRepeatMsg(ulong id, long amount)
        {
            var guild = GetOrCreateGuild(id);
            guild.RepeatMessages = (int) amount;
            GuildCollection.Update(guild);
        }
    }
}