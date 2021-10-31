using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands.Last
{
    public class StreakCommand : LogCommand
    {
        public UserDatabase UserDatabase { get; set; }
        public LastfmClient LastClient { get; set; }

        [SlashCommand("streak", "Return a user's current streak")]
        public async Task Streak(InteractionContext ctx,
            [Option("user", "Whose streaks to check. Leave empty for own account.")]
            DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            user ??= ctx.User;

            if (!UserDatabase.TryGetLast(user.Id, out var last))
                throw new CommandException($"User {user.Mention} has not set their last account.");

            var streaks = await GetStreaksForUser(last);

            if (!string.IsNullOrEmpty(streaks))
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithAuthor($"Current streaks for {user.Username}", $"https://www.last.fm/user/{last}", user.AvatarUrl)
                            .WithDescription(streaks)));
            else
                await ctx.EditResponseAsync(
                    Util.EmbedReply(
                        $"{Constants.InfoEmoji} User {user.Mention} has no ongoing streaks at the moment."));
        }

        private async Task<string> GetStreaksForUser(string last)
        {
            bool cTrack = true, cAlbum = true, cArtist = true;
            int trackCount = 1, albumCount = 1, artistCount = 1;

            var tracks = await LastClient.User.GetRecentScrobbles(last, pagenumber: 1, count: 1000);
            if (!tracks.Success)
                throw new CommandException("last.fm's response was not successful.");
            var firstTrack = tracks.Content[0];
            var currentTrack = firstTrack;

            foreach (var track in tracks.Content.Skip(1))
            {
                if (track.Name == firstTrack.Name && cTrack)
                    trackCount++;
                else
                    cTrack = false;

                if (track.ArtistName == firstTrack.ArtistName && cArtist)
                    artistCount++;
                else
                    cArtist = false;

                if (track.AlbumName == firstTrack.AlbumName && cAlbum)
                    albumCount++;
                else
                    cAlbum = false;

                if (!cAlbum && !cArtist && !cTrack)
                    break;

                currentTrack = track;
            }

            if (cTrack) trackCount = -1;
            if (cArtist) artistCount = -1;
            if (cAlbum) albumCount = -1;

            if (trackCount == 1 && albumCount == 1 && artistCount == 1)
                return null;

            return
                $"{(trackCount == -1 || albumCount == -1 || artistCount == -1 ? "Stopped calculating the streak" : "Streak started")}" +
                $" {Formatter.Timestamp(currentTrack.TimePlayed.GetValueOrDefault(DateTimeOffset.Now))}\n" +
                (trackCount != 1
                    ? $"**Track**: {Formatter.MaskedUrl(firstTrack.Name, firstTrack.Url)} " +
                      $"- {(trackCount == -1 ? "1000+" : trackCount)} Plays\n"
                    : string.Empty) +
                (albumCount != 1
                    ? "**Album**: " +
                      Formatter.MaskedUrl(firstTrack.AlbumName,
                          LastUtil.CleanLastUrl($"{firstTrack.ArtistUrl}/{firstTrack.AlbumName}")) +
                      $" - {(albumCount == -1 ? "1000+" : albumCount)} Plays\n"
                    : string.Empty) +
                (artistCount != 1
                    ? $"**Artist**: {Formatter.MaskedUrl(firstTrack.ArtistName, firstTrack.ArtistUrl)} " +
                      $"- {(artistCount == -1 ? "1000+" : artistCount)} Plays"
                    : string.Empty);
        }
    }
}