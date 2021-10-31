using System.Collections.Generic;

namespace MeiyounaiseSlash.Data.Models
{
    public class Board
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public long Threshold { get; set; }
        public List<ulong> BlacklistedChannels { get; set; }
    }
}