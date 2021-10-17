using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands.Last
{
    public class ServerCommand : LogCommand
    {
        public UserDatabase UserDatabase { get; set; }
        public LastfmClient LastClient { get; set; }

        [SlashCommand("server", "Show every member who's currently scrobbling something.")]
        public async Task Server(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var users = UserDatabase.GetLastUsersInCurrentGuild(ctx.Guild.Members.Keys);

            var currentlyScrobbling =
                await Task.WhenAll(users.Select(x => LastUtil.GetNowPlaying(x.id, x.last, LastClient)));

            var texts = currentlyScrobbling.Where(t => t.Track is not null)
                .Select(np => $"<@{np.Id}> " +
                              Formatter.MaskedUrl("🔊", new Uri($"https://www.last.fm/user/{np.Last}")) + " " +
                              Formatter.MaskedUrl(np.Track.ArtistName, np.Track.ArtistUrl) + " - " +
                              Formatter.MaskedUrl(np.Track.Name, LastUtil.CleanLastUrl(np.Track.Url)))
                .ToList();

            if (texts.Count == 0)
            {
                await ctx.EditResponseAsync(
                    Util.EmbedReply(
                        $"{Constants.ErrorEmoji} No one in this guild is scrobbling something right now."));
                return;
            }

            var embeds = new List<DiscordEmbed>();
            var toAdd = "";

            for (var i = 0; i < texts.Count; i++)
            {
                toAdd += texts[i];
                if (i + 1 != texts.Count)
                    toAdd += "\n⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤⏤\n";

                if (toAdd.Length <= 1800 && texts.Count > i + 1) continue;
                embeds.Add(new DiscordEmbedBuilder()
                    .WithAuthor($"Currently scrobbling in {ctx.Guild.Name}", iconUrl: ctx.Guild.IconUrl)
                    .WithColor(DiscordColor.Red)
                    .WithDescription(toAdd));
                toAdd = "";
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(embeds));
        }
    }
}