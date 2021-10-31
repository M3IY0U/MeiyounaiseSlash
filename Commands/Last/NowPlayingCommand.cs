using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands.Last
{
    public class NowPlayingCommand : LogCommand
    {
        public UserRepository UserRepository { get; set; }
        public LastfmClient LastClient { get; set; }

        [SlashCommand("np", "Returns currently playing or last played song for a user.")]
        public async Task NowPlayingAlias(InteractionContext ctx,
            [Option("user", "The user to fetch, leave blank for own account.")]
            DiscordUser user = null)
        {
            await HandleNowPlayingInteraction(ctx, user);
        }

        [SlashCommand("fm", "Returns currently playing or last played song for a user.")]
        public async Task NowPlaying(InteractionContext ctx,
            [Option("user", "The user to fetch, leave blank for own account.")]
            DiscordUser user = null)
        {
            await HandleNowPlayingInteraction(ctx, user);
        }

        private async Task HandleNowPlayingInteraction(InteractionContext ctx, DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            user ??= ctx.User;
            if (!await UserRepository.TryGetLast(user.Id, out var last))
                throw new CommandException($"User {user.Mention} has not set their last account.");

            var response = await LastClient.User.GetRecentScrobbles(last);
            if (!response.Success || response.Content[0] is null)
                throw new CommandException("last.fm's response was not successful.");

            var info = await LastClient.User.GetInfoAsync(last);

            var embed = BuildEmbed(response.Content[0], info.Content);

            var m = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            foreach (var reaction in await UserRepository.GetReactions(ctx.User.Id))
            {
                await m.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, reaction));
            }
        }


        private static DiscordEmbed BuildEmbed(LastTrack scrobble, LastUser user)
        {
            return new DiscordEmbedBuilder()
                .WithAuthor($"{user.Name} - {(scrobble.IsNowPlaying is null ? "Last Track" : "Now Playing")}",
                    $"https://www.last.fm/user/{user.Name}",
                    scrobble.IsNowPlaying is null
                        ? "https://cdn.discordapp.com/attachments/565956920004050944/752590397590470757/stop.png"
                        : "https://cdn.discordapp.com/attachments/565956920004050944/752590473247457300/play.png")
                .WithColor(new DiscordColor(211, 31, 39))
                .WithDescription(
                    $"{Formatter.MaskedUrl($"**{scrobble.Name}**", scrobble.Url)}\nScrobbled {Formatter.Timestamp(scrobble.TimePlayed ?? DateTimeOffset.Now)}")
                .WithThumbnail(scrobble.Images.Large != null
                    ? scrobble.Images.Large.AbsoluteUri.Replace("/174s/", "/")
                    : Constants.LastFmUnknownAlbum)
                .AddField("Artist",
                    Formatter.MaskedUrl($"**{scrobble.ArtistName}**", scrobble.ArtistUrl),
                    true)
                .AddField("Album",
                    scrobble.AlbumName != ""
                        ? Formatter.MaskedUrl($"**{scrobble.AlbumName}**",
                            LastUtil.CleanLastUrl($"{scrobble.ArtistUrl}/{scrobble.AlbumName}"))
                        : "No album linked on last.fm!", true);
        }
    }
}