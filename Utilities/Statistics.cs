using Prometheus;

namespace MeiyounaiseSlash.Utilities
{
    public static class Statistics
    {
        public static readonly Counter CommandCounter = Metrics
            .CreateCounter("bot_commands_processed", "Number of processed commands.");
        
        public static readonly Counter CommandExceptionCounter = Metrics
            .CreateCounter("bot_commands_errored", "Number of errored commands.");
        
        public static readonly Gauge DiscordServerCount = Metrics
            .CreateGauge("discord_server_count", "Total count of all servers the bot is in");
    }
}