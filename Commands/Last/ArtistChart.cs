using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Services;

namespace MeiyounaiseSlash.Commands.Last
{
    public class ArtistChart : ApplicationCommandModule
    {
        public UserDatabase UserDatabase { get; set; }
        public LastfmClient LastClient { get; set; }
        
        [SlashCommand("artistchart", "Generate an artist chart.")]
        public async Task ArtistChartCommand(InteractionContext ctx,
            [ChoiceProvider(typeof(LastUtil.TimeRangeChoiceProvider))]
            [Option("timerange", "The timerange to fetch scrobbles for.")] string timespan = "overall",
            [Option("user", "The user to fetch, leave blank for own account.")]
            DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            user ??= ctx.User;
            
            if (!UserDatabase.TryGetLast(user.Id, out var last))
                throw new CommandException($"User {user.Mention} has not set their last account.");

            var response = await LastClient.User.GetTopArtists(last, LastUtil.StringToTimeSpan(timespan), 1, 25);
            if (!response.Success)
                throw new CommandException("last.fm's response was not successful.");

            var image = await ArtistChartImageService.GenerateChart(response.Content);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddFile("chart.png", image));
        }
    }
}