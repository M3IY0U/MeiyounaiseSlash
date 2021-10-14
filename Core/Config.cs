namespace MeiyounaiseSlash.Core
{
    public class Config
    {
        public string Token { get; init; }
        public ulong ErrorLogChannel { get; init; }
        public string LastApiKey { get; set; }
        public string LastApiSecret { get; set; }
    }
}