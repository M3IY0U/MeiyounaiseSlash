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
            [Option("amount", "Amount of identical message needed to be repeated.")]
            long amount = -1)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (amount == -1)
            {
                var repeatMessages = GuildDatabase.GetOrCreateGuild(ctx.Guild.Id).RepeatMessages;
                await ctx.EditResponseAsync(
                    Util.EmbedReply($"{Constants.InfoEmoji} Currently repeating messages after {repeatMessages} identical ones."));
                return;
            }
            
            GuildDatabase.SetRepeatMsg(ctx.Guild.Id, amount);
            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Messages will be repeated after {amount} identical messages."));
        }
    }
}