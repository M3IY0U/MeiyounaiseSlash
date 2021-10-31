using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Data.Models;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands.Last
{
    public class UpdateUserCommand : LogCommand
    {
        public UserDatabase UserDatabase { get; set; }
        public LastfmClient LastClient { get; set; }
        public ScrobbleRepository ScrobbleRepository { get; set; }

        public event EventHandler<int> ScrobblesDone;

        [SlashCommand("index", "Fetch all scrobbles for user")]
        public async Task Update(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (!UserDatabase.TryGetLast(ctx.User.Id, out var last))
                throw new CommandException("You have not set your last account yet.");

            await ScrobbleRepository.ClearScrobblesForUserAsync(ctx.User.Id);
            await Task.Delay(2000);
            var info = await LastClient.User.GetInfoAsync(last);

            await ctx.EditResponseAsync(
                Util.EmbedReply(
                    $"{Constants.InfoEmoji} Fetching {last}'s scrobbles...\n({info.Content.Playcount} in total.)"));
            var totalTracks = 0;
            ScrobblesDone += (_, count) =>
            {
                totalTracks += count;
                Console.WriteLine($"{totalTracks}/{info.Content.Playcount}");
            };

            var tasks = new List<Task<IEnumerable<Scrobble>>>();
            var remaining = info.Content.Playcount;
            var page = 1;
            while (remaining > 0)
            {
                tasks.Add(GetScrobblesPerPage(page++, 1000, last, ctx.User.Id));
                remaining -= 1000;
            }

            var allScrobbles = await Task.WhenAll(tasks);
            await ctx.EditResponseAsync(
                Util.EmbedReply(
                    $"{Constants.CheckEmoji} Fetched {totalTracks} scrobbles.\n{Constants.InfoEmoji} Inserting into database..."));
            foreach (var scrobbles in allScrobbles)
            {
                await ScrobbleRepository.AddScrobblesAsync(scrobbles);
            }

            await ScrobbleRepository.SaveChangesAsync();
            await Task.Delay(3000);
            await ctx.EditResponseAsync(
                Util.EmbedReply(
                    $"{Constants.CheckEmoji} Successfully fetched {totalTracks} scrobbles for {ctx.User.Mention}" +
                    $" ({Formatter.MaskedUrl(last, new Uri($"https://www.last.fm/user/{last}"))})."));
        }

        private async Task<IEnumerable<Scrobble>> GetScrobblesPerPage(int page, int count, string user, ulong id)
        {
            var tracks = await LastClient.User.GetRecentScrobbles(user, pagenumber: page, count: count);
            var filteredTracks = tracks.Content
                .Where(w => w.TimePlayed.HasValue && !w.IsNowPlaying.GetValueOrDefault(false)).ToList();

            ScrobblesDone?.Invoke(null, filteredTracks.Count);
            return filteredTracks.Select(t => new Scrobble
            {
                Name = t.Name, AlbumName = t.AlbumName, ArtistName = t.ArtistName,
                TimeStamp = t.TimePlayed.GetValueOrDefault(DateTimeOffset.UnixEpoch), UserId = id
            });
        }
    }
}