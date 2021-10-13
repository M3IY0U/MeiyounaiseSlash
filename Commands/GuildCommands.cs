using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Exceptions;
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
            [Option("amount", "Amount of identical message needed to be repeated. Set to 0 to disable.")]
            long amount = -1)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (amount == -1)
            {
                var repeatMessages = GuildDatabase.GetOrCreateGuild(ctx.Guild.Id).RepeatMessages;
                await ctx.EditResponseAsync(
                    Util.EmbedReply(
                        $"{Constants.InfoEmoji} Currently repeating messages after {repeatMessages} identical ones."));
                return;
            }

            GuildDatabase.SetRepeatMsg(ctx.Guild.Id, amount);
            await ctx.EditResponseAsync(
                Util.EmbedReply(
                    $"{Constants.CheckEmoji} Messages will be repeated after {amount} identical messages."));
        }

        [SlashCommand("joinchannel", "Set the channel the bot posts join messages in.")]
        public async Task SetJoinChannel(InteractionContext ctx,
            [Option("channel", "The channel to post join messages in.")]
            DiscordChannel channel)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (channel.Type != ChannelType.Text)
                throw new CommandException("Only text channels can be used for this functionality.");
            GuildDatabase.SetChannel(ctx.Guild.Id, channel.Id, true);

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Join messages will now be posted in {channel.Mention}"));
        }

        [SlashCommand("leavechannel", "Set the channel the bot posts leave messages in.")]
        public async Task SetLeaveChannel(InteractionContext ctx,
            [Option("channel", "The channel to post leave messages in.")]
            DiscordChannel channel)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (channel.Type != ChannelType.Text)
                throw new CommandException("Only text channels can be used for this functionality.");
            GuildDatabase.SetChannel(ctx.Guild.Id, channel.Id, false);

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Leave messages will now be posted in {channel.Mention}"));
        }

        [SlashCommand("joinmessage", "Set the message used when a member joins.")]
        public async Task SetJoinMessage(InteractionContext ctx,
            [Option("message", "Message to be sent [user] will be replaced with a mention of the user")]
            string message)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildDatabase.SetMessage(ctx.Guild.Id, message, true);

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Join message set to: `{message}`"));
        }

        [SlashCommand("leavemessage", "Set the message used when a member leaves.")]
        public async Task SetLeaveMessage(InteractionContext ctx,
            [Option("message", "Message to be sent [user] will be replaced with a mention of the user")]
            string message)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildDatabase.SetMessage(ctx.Guild.Id, message, false);

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Leave message set to: `{message}`"));
        }

        [SlashCommand("disable", "Disable a guild-specific funtionality")]
        public async Task Disable(InteractionContext ctx, [Option("functionality", "Functionality to disable")]
            DisableOption functionality)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            string response;
            switch (functionality)
            {
                case DisableOption.RepeatMsg:
                    GuildDatabase.SetRepeatMsg(ctx.Guild.Id, 0);
                    response = "Messages will no longer be repeated.";
                    break;
                case DisableOption.JoinMsg:
                    GuildDatabase.SetChannel(ctx.Guild.Id, 0, true);
                    response = "Join messages will no longer be sent.";
                    break;
                case DisableOption.LeaveMsg:
                    GuildDatabase.SetChannel(ctx.Guild.Id, 0, false);
                    response = "Leave messages will no longer be sent";
                    break;
                default:
                    throw new CommandException("This exception should never be thrown");
            }
            
            await ctx.EditResponseAsync(Util.EmbedReply($"{Constants.CheckEmoji} {response}"));
        }

        public enum DisableOption
        {
            [ChoiceName("repeatmsg")] RepeatMsg,
            [ChoiceName("joinmessage")] JoinMsg,
            [ChoiceName("leavemessage")] LeaveMsg,
        }
    }
}