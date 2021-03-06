using System.Collections.Generic;

namespace MeiyounaiseSlash.Data.Models
{
    public class Guild
    {
        public ulong Id { get; init; }
        public string JoinMessage { get; set; }
        public string LeaveMessage { get; set; }
        public ulong JoinChannel { get; set; }
        public ulong LeaveChannel { get; set; }
        public int RepeatMessages { get; set; }
        public ulong PinArchiveChannel { get; set; }
        public Dictionary<ulong, List<ulong>> PinnedMessages { get; set; }
    }
}