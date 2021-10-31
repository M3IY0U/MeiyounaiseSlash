namespace MeiyounaiseSlash.Core
{
    public class Config
    {
        public string Token { get; init; }
        public ulong ErrorLogChannel { get; init; }
        public string LastApiKey { get; set; }
        public string LastApiSecret { get; set; }
        public string SpotifyClientId { get; set; }
        public string SpotifyClientSecret { get; set; }
        public string ConnectionString { get; set; }
    }
}