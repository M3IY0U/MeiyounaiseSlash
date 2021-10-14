using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands.Last
{
    public class Account : ApplicationCommandModule
    {
        public UserDatabase UserDatabase { get; set; }

        [SlashCommand("setlast", "Set your last.fm account.")]
        public async Task SetLast(InteractionContext ctx,
            [Option("lastfm", "Your last.fm account name", true)] string last = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            string content;
            if (string.IsNullOrEmpty(last))
            {
                content = UserDatabase.TryGetLast(ctx.User.Id, out last)
                    ? $"{Constants.InfoEmoji} Your last.fm account is currently set to: `{last}`"
                    : $"{Constants.ErrorEmoji} You have not set your last.fm account yet.";
            }
            else
            {
                UserDatabase.SetLastAccount(ctx.User.Id, last);
                content = $"{Constants.CheckEmoji} Your last.fm account has been set to: `{last}`";
            }

            await ctx.EditResponseAsync(Util.EmbedReply(content));
        }
    }
}