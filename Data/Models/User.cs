using System.Collections.Generic;

namespace MeiyounaiseSlash.Data.Models
{
    public class User
    {
        public ulong Id { get; init; }
        public string LastFm { get; set; }
        public HashSet<string> NowPlayingReactions { get; set; } = new ();
    }
}