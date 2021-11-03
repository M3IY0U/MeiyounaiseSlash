using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;

namespace MeiyounaiseSlash.Commands.Last
{
    public class MilestoneCommand : LogCommand
    {
        public UserRepository UserRepository { get; set; }
        public LastfmClient LastClient { get; set; }
        public ScrobbleRepository ScrobbleRepository { get; set; }

        [SlashCommand("ms", "Get the nth track you scrobbled.")]
        public async Task Milestone(InteractionContext ctx,
            [Option("number", "The nth scrobble you want to see")]
            long number = 1,
            [Option("user", "The nth scrobble you want to see")]
            DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            user ??= ctx.User;
            if (await UserRepository.GetLastIndexed(user.Id) == DateTime.UnixEpoch)
                throw new CommandException($"{user.Mention} has not indexed their scrobbles yet.");
            await UserRepository.TryGetLast(user.Id, out var last);

            number = Math.Clamp(number, 1, ScrobbleRepository.GetScrobbleCountForUser(user.Id));
            var scrobble = ScrobbleRepository.GetNthScrobble(user.Id, (int) number);

            var info = await LastClient.Track.GetInfoAsync(scrobble.Name, scrobble.ArtistName);
            var eb = new DiscordEmbedBuilder()
                .WithThumbnail(scrobble.CoverUrl)
                .WithColor(ctx.Member.Color)
                .WithAuthor($"Scrobble #{number} for {user.Username}:",
                    $"https://www.last.fm/user/{last}", user.AvatarUrl)
                .WithDescription(
                    $"**{Formatter.MaskedUrl(scrobble.Name, info.Content.Url)}**\n\n" +
                    $"Obtained {Formatter.Timestamp(scrobble.TimeStamp)}")
                .AddField("Artist",
                    Formatter.MaskedUrl($"**{scrobble.ArtistName}**", info.Content.ArtistUrl), true)
                .AddField("Album",
                    scrobble.AlbumName != ""
                        ? Formatter.MaskedUrl($"**{scrobble.AlbumName}**",
                            LastUtil.CleanLastUrl($"{info.Content.ArtistUrl}/{scrobble.AlbumName}"))
                        : "No album linked on last.fm!", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(eb));
        }
    }
}