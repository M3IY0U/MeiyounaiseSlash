using System;

namespace MeiyounaiseSlash.Data.Models
{
    public class Scrobble
    {
        public int Id { get; set; }
        public string ArtistName { get; set; }
        public string Name { get; set; }
        public string AlbumName { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public ulong UserId { get; set; }
        public string CoverUrl { get; set; }
    }
}