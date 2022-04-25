using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands
{
    [SlashCommandGroup("board", "Commands related to emoji boards")]
    [SlashRequireUserPermissions(Permissions.ManageChannels)]
    public class BoardCommands : LogCommand
    {
        public BoardRepository BoardRepository { get; set; }

        [SlashCommand("create", "Create a board.")]
        public async Task CreateBoardCommand(InteractionContext ctx,
            [Option("channel", "The new board channel")]
            DiscordChannel channel,
            [Option("threshold", "Amount of reactions needed to post a message in the board")]
            long threshold)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (channel.Type != ChannelType.Text)
                throw new CommandException("Only text channels can be boards.");

            if (BoardRepository.UpsertBoard(ctx.Guild.Id, channel.Id, threshold))
            {
                await ctx.EditResponseAsync(Util.EmbedReply(
                    $"{Constants.CheckEmoji} Board in channel {channel.Mention} with {threshold} reactions has been created."));    
            }
            else
            {
                await ctx.EditResponseAsync(Util.EmbedReply(
                    $"{Constants.CheckEmoji} The board in this server was updated to: {channel.Mention} with {threshold} reactions."));    
            }
        }

        [SlashCommand("delete", "Delete a board.")]
        public async Task DeleteBoardCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

            if (!BoardRepository.TryGetBoard(b => b.GuildId == ctx.Guild.Id, out var board))
                throw new CommandException("There is no active board in this server.");

            var buttons = new[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, "delete_board_yes", "Yes"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "delete_board_no", "No")
            };

            var msg = await ctx.EditResponseAsync(Util.EmbedReply(
                    $"{Constants.InfoEmoji} Do you *really* want to delete the board in <#{board.ChannelId}>?")
                // ReSharper disable once CoVariantArrayConversion
                .AddComponents(buttons));
            
            var ir = await ctx.Client.GetInteractivity()
                .WaitForButtonAsync(msg, buttons);

            if (ir.TimedOut) throw new InteractionTimeoutException("Timed out.");

            if (ir.Result.Id == "delete_board_yes")
            {
                BoardRepository.DeleteBoard(ctx.Guild.Id, board.ChannelId);
                await ctx.EditResponseAsync(
                    Util.EmbedReply($"{Constants.CheckEmoji} The board in <#{board.ChannelId}> was deleted."));
            }
            else
            {
                await ctx.EditResponseAsync(Util.EmbedReply($"{Constants.InfoEmoji} Aborted."));
            }
        }
        
        [SlashCommand("blacklist", "Adds a channel to the blacklist of the board.")]
        public async Task BlackList(InteractionContext ctx,
            [Option("action", "Action to take.")] BlacklistAction action,
            [Option("channel", "Target channel.")] DiscordChannel channel = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            switch (action)
            {
                case BlacklistAction.add:
                    if (channel?.Type is not ChannelType.Text && channel?.Type is not ChannelType.PublicThread)
                        throw new CommandException("Invalid channel provided.");
                    if (!BoardRepository.TryGetBoard(b => b.GuildId == ctx.Guild.Id,
                        out _))
                        throw new CommandException("There currently is no board in the provided channel.");

                    BoardRepository.AddToBlackList(ctx.Guild.Id, channel.Id);
                    await ctx.EditResponseAsync(Util.EmbedReply(
                        $"{Constants.CheckEmoji} Channel {channel.Mention} has been added to the blacklist."));

                    break;
                case BlacklistAction.remove:
                    if (channel is null)
                        throw new CommandException("You need to provide a channel to remove.");
                    if (!BoardRepository.TryGetBoard(b => b.GuildId == ctx.Guild.Id,
                        out var board))
                        throw new CommandException("There currently is no board in the provided channel.");
                    if (!board.BlacklistedChannels.Contains(channel.Id))
                        throw new CommandException("The board doesn't have that channel blacklisted.");

                    BoardRepository.RemoveFromBlacklist(ctx.Guild.Id, channel.Id);
                    await ctx.EditResponseAsync(Util.EmbedReply(
                        $"{Constants.CheckEmoji} Channel {channel.Mention} has been removed from the blacklist."));

                    break;
                case BlacklistAction.clear:
                    if (!BoardRepository.TryGetBoard(b => b.GuildId == ctx.Guild.Id, out _))
                        throw new CommandException("There currently is no board in the provided channel.");

                    var buttons = new[]
                    {
                        new DiscordButtonComponent(ButtonStyle.Success, "confirm_blacklist_clear", "Clear"),
                        new DiscordButtonComponent(ButtonStyle.Danger, "confirm_blacklist_abort", "Abort")
                    };

                    var m = await ctx.EditResponseAsync(Util.EmbedReply(
                            $"{Constants.InfoEmoji} Do you *really* want to clear all blacklisted channels?")
                        // ReSharper disable once CoVariantArrayConversion
                        .AddComponents(buttons));

                    var x = await ctx.Client.GetInteractivity()
                        .WaitForButtonAsync(m, buttons);
                    
                    if (x.TimedOut) throw new InteractionTimeoutException("Timed out.");

                    if (x.Result.Id == "confirm_blacklist_clear")
                    {
                        BoardRepository.ClearBlacklist(ctx.Guild.Id);
                        await ctx.EditResponseAsync(
                            Util.EmbedReply($"{Constants.CheckEmoji} Blacklist has been cleared."));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(Util.EmbedReply($"{Constants.InfoEmoji} Clearing aborted."));
                    }

                    break;
                case BlacklistAction.list:
                    if (!BoardRepository.TryGetBoard(b => b.GuildId == ctx.Guild.Id,
                        out board))
                        throw new CommandException("There currently is no board in the provided channel.");

                    await ctx.EditResponseAsync(Util.EmbedReply($"{Constants.InfoEmoji} Blacklisted channels:\n" +
                                                                string.Join("\n",
                                                                    board.BlacklistedChannels.Select(b => $"» <#{b}>"))));

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
        
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum BlacklistAction
        {
            [ChoiceName("add")] add,
            [ChoiceName("remove")] remove,
            [ChoiceName("clear")] clear,
            [ChoiceName("list")] list
        }
    }
}