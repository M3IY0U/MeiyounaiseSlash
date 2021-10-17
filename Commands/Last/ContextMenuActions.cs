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

namespace MeiyounaiseSlash.Commands.Last
{
    public class ContextMenuActions : LogCommand
    {
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Youtube Search")]
        public async Task YoutubeSearch(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            string query;
            if (ctx.TargetMessage.Author == ctx.Client.CurrentUser
                && string.IsNullOrEmpty(ctx.TargetMessage.Content)
                && ctx.TargetMessage.Embeds.Count == 1
                && ctx.TargetMessage.Embeds[0].Description.Contains("Scrobbled"))
            {
                var (title, artist) = (
                    ctx.TargetMessage.Embeds[0].Description[..ctx.TargetMessage.Embeds[0].Description.IndexOf('\n')],
                    ctx.TargetMessage.Embeds[0].Fields[0].Value);

                query =
                    $"{Formatter.Strip(artist)[..(artist.IndexOf("https://www.last.fm", StringComparison.Ordinal) - 7)]} " +
                    $"{Formatter.Strip(title)[..(title.IndexOf("https://www.last.fm", StringComparison.Ordinal) - 7)]}";
            }
            else
                query = ctx.TargetMessage.Content;

            try
            {
                using var httpClient = new HttpClient();
                var client = new DefaultSearchClient(new YoutubeSearchBackend());
                var result = await client.SearchAsync(httpClient, query, 1);
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Query: `{query}`\n{result.Results.First().Url}"));
            }
            catch (NoResultFoundException)
            {
                await ctx.EditResponseAsync(
                    Util.EmbedReply(
                        $"{Constants.ErrorEmoji} Nothing found using query `{ctx.TargetMessage.Content}`."));
            }
        }
    }
}