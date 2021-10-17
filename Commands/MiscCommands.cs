using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MeiyounaiseSlash.Utilities;
using YoutubeSearchApi.Net;
using YoutubeSearchApi.Net.Backends;
using YoutubeSearchApi.Net.Exceptions;

namespace MeiyounaiseSlash.Commands
{
    public class MiscCommands : LogCommand
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
        }

        [SlashCommand("yt", "Search YouTube.")]
        public async Task YoutubeCommand(InteractionContext ctx,
            [Option("query", "What to search for.")] string query)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            await ctx.EditResponseAsync(await SearchYoutube(query));
        }

        [SlashCommand("youtube", "Search YouTube.")]
        public async Task YoutubeAlias(InteractionContext ctx,
            [Option("query", "What to search for.")] string query)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            await ctx.EditResponseAsync(await SearchYoutube(query));
        }

        private static async Task<DiscordWebhookBuilder> SearchYoutube(string query)
        {
            try
            {
                using var httpClient = new HttpClient();
                var client = new DefaultSearchClient(new YoutubeSearchBackend());
                var result = await client.SearchAsync(httpClient, query, 1);
                return new DiscordWebhookBuilder().WithContent($"Query: `{query}`\n{result.Results.First().Url}");
            }
            catch (NoResultFoundException)
            {
                return Util.EmbedReply($"{Constants.ErrorEmoji} Nothing found using query `{query}`.");
            }
        }
    }
}