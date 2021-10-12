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
            public int PreviousMessages { get; set; }
        }

        #endregion
        
        
        private ILiteCollection<Guild> _guildCollection;
        
        public GuildDatabase(string path) : base(path)
        {
            _guildCollection = Database.GetCollection<Guild>("guilds");
        }
    }
}