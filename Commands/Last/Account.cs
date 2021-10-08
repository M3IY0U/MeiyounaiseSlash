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

        [SlashCommand("set", "Set your last.fm account.")]
        public async Task SetLast(InteractionContext ctx,
            [Option("lastfm", "Your last.fm account name", true)] string last = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true));
            
            var response = new DiscordWebhookBuilder();

            if (string.IsNullOrEmpty(last))
            {
                last = UserDatabase.GetLastAccount(ctx.User.Id);
                if (string.IsNullOrEmpty(last))
                {
                    await ctx.EditResponseAsync(
                        response.AddEmbed(new DiscordEmbedBuilder()
                            .WithDescription($"{Constants.ErrorEmoji} You have not set your last.fm account yet.")));
                }
                else
                {
                    await ctx.EditResponseAsync(
                        response.AddEmbed(new DiscordEmbedBuilder()
                            .WithDescription($"{Constants.InfoEmoji} Your last.fm account is currently set to: `{last}`")));
                }
            }
            else
            {
                UserDatabase.SetLastAccount(ctx.User.Id, last);
                await ctx.EditResponseAsync(
                    response.AddEmbed(new DiscordEmbedBuilder()
                        .WithDescription($"{Constants.CheckEmoji} Your last.fm account has been set to: `{last}`")));
            }
        }
    }
}