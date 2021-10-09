using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands
{
    public class MiscCommands : ApplicationCommandModule
    {
        
        [SlashCommand("ping", "Returns bot latencies")]
        public async Task PingCommand(InteractionContext ctx)
        {
            var ts = DateTime.Now;
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Ping"));

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Pong!").AddEmbed(
                new DiscordEmbedBuilder()
                    .AddField("Client Latency", $"{ctx.Client.Ping}ms", true)
                    .AddField("Edit Latency", $"{(DateTime.Now - ts).Milliseconds}ms", true)));
            
            Statistics.CommandCounter.Inc();
        }
    }
}