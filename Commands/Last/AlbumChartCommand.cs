using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Services.Charts;
using static MeiyounaiseSlash.Commands.Last.LastUtil;

namespace MeiyounaiseSlash.Commands.Last
{
    public class AlbumChartCommand : LogCommand
    {
        public UserRepository UserRepository { get; set; }
        public LastfmClient LastClient { get; set; }
        
        [SlashCommand("albumchart", "Generate an album chart.")]
        public async Task AlbumChart(InteractionContext ctx,
            [Option("timerange", "The timerange to fetch scrobbles for.")] TimeSpan timespan = TimeSpan.Overall,
            [Option("user", "The user to fetch, leave blank for own account.")]
            DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            user ??= ctx.User;
            
            if (!await UserRepository.TryGetLast(user.Id, out var last))
                throw new CommandException($"User {user.Mention} has not set their last account.");

            var response = await LastClient.User.GetTopAlbums(last, EnumToTimeSpan(timespan), 1, 25);
            if (!response.Success)
                throw new CommandException("last.fm's response was not successful.");

            var image = AlbumChartImageService.GenerateChart(response.Content);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddFile("chart.png", image));
        }
    }
}