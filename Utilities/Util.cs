using DSharpPlus.Entities;

namespace MeiyounaiseSlash.Utilities
{
    public static class Util
    {
        public static DiscordWebhookBuilder EmbedReply(string content) =>
            new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder().WithDescription(content).Build());
    }
}