using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Services.Charts;

namespace MeiyounaiseSlash.Commands.Last
{
    public class ArtistGraphCommand : LogCommand
    {
        public UserRepository UserRepository { get; set; }
        public LastfmClient LastClient { get; set; }

        [SlashCommand("trendgraph", "Generate an artist graph.")]
        public async Task ArtistChart(InteractionContext ctx,
            [Option("artist", "Leave empty for general scrobble trend. Put artist for specific artist trend")]
            string artist = "",
            [Option("user", "The user to fetch, leave blank for own account.")]
            DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            user ??= ctx.User;

            if (!await UserRepository.TryGetLast(user.Id, out var last))
                throw new CommandException($"User {user.Mention} has not set their last account.");

            var response = await LastClient.User.GetRecentScrobbles(last, DateTimeOffset.UtcNow.AddDays(-7),
                DateTimeOffset.UtcNow, true, 1, 1000);

            if (!response.Success)
                throw new CommandException("last.fm's response was not successful.");

            if (!string.IsNullOrEmpty(artist))
            {
                var artistResponse = await LastClient.Artist.GetInfoAsync(artist, autocorrect: true);
                if (!artistResponse.Success)
                    throw new CommandException("No artist with that name could be found on last.");
                artist = artistResponse.Content.Name;
            }

            var image = await ArtistGraphImageService.GenerateChart(artist, response.Content);
            var totalScrobbles = response.Content.Count;

            var footer = string.IsNullOrEmpty(artist)
                ? $"{totalScrobbles} total scrobbles this week"
                : $"{response.Content.Count(s => s.ArtistName == artist)} {artist} scrobbles out of {totalScrobbles} total scrobbles this week";

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddFile("chart.png", image)
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithImageUrl("attachment://chart.png")
                    .WithFooter(footer)
                    .WithAuthor(
                        $"Weekly {(string.IsNullOrEmpty(artist) ? "" : artist + " ")}Plays for {ctx.Member.DisplayName}",
                        $"https://www.last.fm/user/{last}")));
        }
    }
}