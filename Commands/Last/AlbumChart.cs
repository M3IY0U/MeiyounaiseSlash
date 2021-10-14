using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Services;

namespace MeiyounaiseSlash.Commands.Last
{
    public class AlbumChart : ApplicationCommandModule
    {
        public UserDatabase UserDatabase { get; set; }
        public LastfmClient LastClient { get; set; }
        
        [SlashCommand("albumchart", "Generate an album chart.")]
        public async Task AlbumChartCommand(InteractionContext ctx,
            [Option("user", "The user to fetch, leave blank for own account.")]
            DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            user ??= ctx.User;
            
            if (!UserDatabase.TryGetLast(user.Id, out var last))
                throw new CommandException($"User {user.Mention} has not set their last account.");

            var response = await LastClient.User.GetTopAlbums(last, LastStatsTimeSpan.Overall, 1, 25);
            if (!response.Success)
                throw new CommandException("last.fm's response was not successful.");

            var image = AlbumChartImageService.GenerateChart(response.Content);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddFile("chart.png", image));
        }
        
    }
}