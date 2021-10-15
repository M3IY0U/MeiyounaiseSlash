using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Exceptions;

namespace MeiyounaiseSlash.Commands.Last
{
    public class Recent : ApplicationCommandModule
    {
        public UserDatabase UserDatabase { get; set; }
        public LastfmClient LastClient { get; set; }

        [SlashCommand("recent", "Returns most recent scrobbles.")]
        public async Task RecentCommand(InteractionContext ctx,
            [Option("amount", "Amount of recent scrobbles to show (max 10, default 5)")]
            long amount = 5,
            [Option("user", "User to get recent scrobbles for, leave blank for own account.")]
            DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            user ??= ctx.User;

            if (!UserDatabase.TryGetLast(user.Id, out var last))
                throw new CommandException($"User {user.Mention} has not set their last account.");

            var response = await LastClient.User.GetRecentScrobbles(last);
            if (!response.Success)
                throw new CommandException("last.fm's response was not successful.");

            var eb = new DiscordEmbedBuilder()
                .WithAuthor($"{last}'s most recent scrobbles", $"https://www.last.fm/user/{last}", user.AvatarUrl)
                .WithColor(DiscordColor.Red)
                .WithThumbnail(response.Content[0].Images.Large != null
                    ? response.Content[0].Images.Large.AbsoluteUri.Replace("/174s/", "/")
                    : "https://lastfm.freetls.fastly.net/i/u/174s/c6f59c1e5e7240a4c0d427abd71f3dbb");

            if (amount > 10)
                amount = 10;

            foreach (var track in response.Content.Take((int) amount))
            {
                var ago = Formatter.Timestamp(track.TimePlayed ?? DateTime.Now);
                eb.AddField(ago, string.Concat(
                    $"[{track.ArtistName}](https://www.last.fm/music/{track.ArtistName.Replace(" ", "+").Replace("(", "\\(").Replace(")", "\\)")})",
                    " - ",
                    $"[{track.Name}]({track.Url.ToString().Replace("(", "\\(").Replace(")", "\\)").Replace("　", "%E3%80%80")})"));
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(eb));
        }
    }
}