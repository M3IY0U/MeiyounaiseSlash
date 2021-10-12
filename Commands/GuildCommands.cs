using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands
{
    [SlashCommandGroup("guild", "Commands related to guild specific funcionality")]
    [SlashRequireUserPermissions(Permissions.ManageMessages)]
    public class GuildCommands : ApplicationCommandModule
    {
        public GuildDatabase GuildDatabase { get; set; }

        [SlashCommand("repeatmsg", "Change if the bot repeats your message after a set amount")]
        public async Task RepeatMessageConfig(InteractionContext ctx,
            [Option("amount", "Amount of equal message needed to be repeated.")]
            long amount)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildDatabase.SetRepeatMsg(ctx.Guild.Id, amount);
            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.InfoEmoji} Messages will be repeated after {amount} equal messages."));
        }
    }
}