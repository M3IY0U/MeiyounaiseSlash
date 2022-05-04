using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Services;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands
{
    [SlashCommandGroup("guild", "Commands related to guild specific funcionality")]
    [SlashRequireUserPermissions(Permissions.ManageMessages)]
    public class GuildCommands : LogCommand
    {
        public GuildRepository GuildRepository { get; set; }

        [SlashCommand("repeatmsg", "Change if the bot repeats your message after a set amount")]
        public async Task RepeatMessageConfig(InteractionContext ctx,
            [Option("amount", "Amount of identical message needed to be repeated. Set to 0 to disable.")]
            long amount = -1)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (amount == -1)
            {
                var guild = await GuildRepository.GetOrCreateGuild(ctx.Guild.Id);
                await ctx.EditResponseAsync(
                    Util.EmbedReply(
                        $"{Constants.InfoEmoji} Currently repeating messages after {guild.RepeatMessages} identical ones."));
                return;
            }

            await GuildRepository.SetRepeatMsg(ctx.Guild.Id, amount);
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
            await GuildRepository.SetChannel(ctx.Guild.Id, channel.Id, GuildRepository.ChannelType.Join);

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
            await GuildRepository.SetChannel(ctx.Guild.Id, channel.Id, GuildRepository.ChannelType.Leave);

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Leave messages will now be posted in {channel.Mention}"));
        }

        [SlashCommand("joinmessage", "Set the message used when a member joins.")]
        public async Task SetJoinMessage(InteractionContext ctx,
            [Option("message", "Message to be sent [user] will be replaced with a mention of the user")]
            string message)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            await GuildRepository.SetMessage(ctx.Guild.Id, message, true);

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Join message set to: `{message}`"));
        }

        [SlashCommand("leavemessage", "Set the message used when a member leaves.")]
        public async Task SetLeaveMessage(InteractionContext ctx,
            [Option("message", "Message to be sent [user] will be replaced with a mention of the user")]
            string message)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            await GuildRepository.SetMessage(ctx.Guild.Id, message, false);

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Leave message set to: `{message}`"));
        }

        [SlashCommand("pinarchive", "Set the channel to be used as pin archive (unpinned messages get posted there)")]
        public async Task SetPinArchive(InteractionContext ctx,
            [Option("channel", "The channel to post old pins in.")]
            DiscordChannel channel)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (channel.Type != ChannelType.Text)
                throw new CommandException("Only text channels can be used for this functionality.");
            await GuildRepository.SetChannel(ctx.Guild.Id, channel.Id, GuildRepository.ChannelType.PinArchive);
            
            await GuildRepository.InitArchive(ctx.Guild.Id, await GuildService.GetPinnedMessagesInGuild(ctx.Guild));

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} Unpinned messages will now be posted in {channel.Mention}"));
        }

        [SlashCommand("disable", "Disable a guild-specific funtionality")]
        public async Task Disable(InteractionContext ctx,
            [Option("functionality", "Functionality to disable")] DisableOption functionality)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            string response;
            switch (functionality)
            {
                case DisableOption.RepeatMsg:
                    await GuildRepository.SetRepeatMsg(ctx.Guild.Id, 0);
                    response = "Messages will no longer be repeated.";
                    break;
                case DisableOption.JoinMsg:
                    await GuildRepository.SetChannel(ctx.Guild.Id, 0, GuildRepository.ChannelType.Join);
                    response = "Join messages will no longer be sent.";
                    break;
                case DisableOption.LeaveMsg:
                    await GuildRepository.SetChannel(ctx.Guild.Id, 0, GuildRepository.ChannelType.Leave);
                    response = "Leave messages will no longer be sent.";
                    break;
                case DisableOption.PinArchive:
                    await GuildRepository.SetChannel(ctx.Guild.Id, 0, GuildRepository.ChannelType.PinArchive);
                    response = "Unpinned messages will no longer be posted.";
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
            [ChoiceName("pinarchive")] PinArchive,
        }
    }
}