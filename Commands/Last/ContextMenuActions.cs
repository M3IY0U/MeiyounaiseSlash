using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MeiyounaiseSlash.Utilities;
using SpotifyAPI.Web;
using YoutubeSearchApi.Net;
using YoutubeSearchApi.Net.Backends;
using YoutubeSearchApi.Net.Exceptions;

namespace MeiyounaiseSlash.Commands.Last
{
    public class ContextMenuActions : LogCommand
    {
        private SpotifyClient Spotify { get; set; }

        public ContextMenuActions(SpotifyClient spotifyClient)
        {
            Spotify = spotifyClient;
        }

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Youtube Search")]
        public async Task YoutubeSearch(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                var query = BuildQuery(ctx);
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

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Spotify Search")]
        public async Task SpotifySearch(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);


            try
            {
                var result = string.Empty;
                var query = BuildQuery(ctx);
                var x = await Spotify.Search.Item(new SearchRequest(SearchRequest.Types.All, query));
                if (x.Tracks.Total > 0)
                    result = x.Tracks.Items?[0].ExternalUrls.First().Value;
                else if (x.Albums.Total > 0)
                    result = x.Albums.Items?[0].ExternalUrls.First().Value;
                else if (x.Artists.Total > 0)
                    result = x.Artists.Items?[0].ExternalUrls.First().Value;
                if (string.IsNullOrEmpty(result))
                    throw new NoResultFoundException();
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Query: `{query}`\n{result}"));
            }
            catch (NoResultFoundException)
            {
                await ctx.EditResponseAsync(
                    Util.EmbedReply(
                        $"{Constants.ErrorEmoji} Nothing found using query `{ctx.TargetMessage.Content}`."));
            }
        }

        private static string BuildQuery(ContextMenuContext ctx)
        {
            if (ctx.TargetMessage.Author != ctx.Client.CurrentUser
                || !string.IsNullOrEmpty(ctx.TargetMessage.Content)
                || ctx.TargetMessage.Embeds.Count != 1
                || !ctx.TargetMessage.Embeds[0].Description.Contains("Scrobbled"))
                return ctx.TargetMessage.Content;

            var (title, artist) = (
                ctx.TargetMessage.Embeds[0].Description[..ctx.TargetMessage.Embeds[0].Description.IndexOf('\n')],
                ctx.TargetMessage.Embeds[0].Fields[0].Value);

            return
                $"{Formatter.Strip(artist)[..(artist.IndexOf("https://www.last.fm", StringComparison.Ordinal) - 7)]} " +
                $"{Formatter.Strip(title)[..(title.IndexOf("https://www.last.fm", StringComparison.Ordinal) - 7)]}";
        }
    }
}