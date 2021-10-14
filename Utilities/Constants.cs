using DSharpPlus.Entities;

namespace MeiyounaiseSlash.Utilities
{
    public static class Constants
    {
        public static readonly DiscordEmoji CheckEmoji = DiscordEmoji.FromUnicode("✅");
        public static readonly DiscordEmoji InfoEmoji = DiscordEmoji.FromUnicode("ℹ");
        public static readonly DiscordEmoji ErrorEmoji = DiscordEmoji.FromUnicode("❎");
        public const string LastFmUnknownAlbum = "https://lastfm.freetls.fastly.net/i/u/c6f59c1e5e7240a4c0d427abd71f3dbb";
        public const string LastFmUnknownArtist = "https://lastfm.freetls.fastly.net/i/u/avatar/2a96cbd8b46e442fc41c2b86b821562f";
        public static ulong? ErrorLogChannel { get; set; }
    }
}