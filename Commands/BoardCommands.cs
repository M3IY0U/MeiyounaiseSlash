using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands
{
    [SlashCommandGroup("board", "Commands related to emoji boards")]
    [SlashRequireUserPermissions(Permissions.ManageChannels)]
    public class BoardCommands : ApplicationCommandModule
    {
        public BoardDatabase BoardDatabase { get; set; }

        [SlashCommand("create", "Create a board.")]
        public async Task CreateBoardCommand(InteractionContext ctx,
            [Option("channel", "The new board channel")]
            DiscordChannel channel,
            [Option("amount", "Amount of reactions needed to post a message in the board")]
            long amount)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (channel.Type != ChannelType.Text)
                throw new CommandException("Only text channels can be boards.");

            BoardDatabase.AddBoard(ctx.Guild.Id, channel.Id, amount);

            await ctx.EditResponseAsync(Util.EmbedReply(
                $"{Constants.CheckEmoji} Board in channel {channel.Mention} with {amount} reactions has been created."));
        }

        [SlashCommand("delete", "Delete a board.")]
        public async Task DeleteBoardCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var boards = BoardDatabase.GetBoardsInGuild(ctx.Guild.Id)
                .Select(board => (board, name: ctx.Guild.GetChannel(board.Channel).Name)).ToList();

            if (!boards.Any())
            {
                await ctx.EditResponseAsync(
                    Util.EmbedReply($"{Constants.ErrorEmoji} No active boards in this server."));
                return;
            }

            var options = boards.Select(tuple =>
                new DiscordSelectComponentOption($"#{tuple.name} ({tuple.board.AmountNeeded} reactions)",
                    $"{tuple.board.Channel}"));

            var m = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Select the board you want to delete:")
                .AddComponents(new DiscordSelectComponent("board_delete", "Example Board", options)));

            var result = await ctx.Client.GetInteractivity().WaitForSelectAsync(m, ctx.User, "board_delete");

            BoardDatabase.DeleteBoard(ctx.Guild.Id, Convert.ToUInt64(result.Result.Values[0]));

            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.CheckEmoji} The board in <#{result.Result.Values[0]}> was deleted."));
        }

        [SlashCommand("list", "List board(s) in this server.")]
        public async Task ListBoardsCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var boards = BoardDatabase.GetBoardsInGuild(ctx.Guild.Id).Select(x =>
                $"» <#{x.Channel}> with {x.AmountNeeded} reactions\n\tBlacklisted channels: " +
                $"{string.Join(", ", x.BlacklistedChannels.Select(c => $"<#{c}>"))}");
            var content = string.Join("\n", boards);
            await ctx.EditResponseAsync(
                Util.EmbedReply($"{Constants.InfoEmoji} Board(s) in {ctx.Guild.Name}:\n{content}"));
        }

        [SlashCommand("blacklist", "Adds a channel to the blacklist of the board.")]
        public async Task BlackList(InteractionContext ctx,
            [Option("action", "Action to take.")] BlacklistAction action,
            [Option("boardchannel", "The board channel.")]
            DiscordChannel boardChannel,
            [Option("channel", "Target channel.")] DiscordChannel channel = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            switch (action)
            {
                case BlacklistAction.Add:
                    if (channel?.Type is not ChannelType.Text)
                        throw new CommandException("Invalid channel provided.");
                    if (!BoardDatabase.TryGetBoard(b => b.GuildId == ctx.Guild.Id && b.Channel == boardChannel.Id,
                        out _))
                        throw new CommandException("There currently is no board in the provided channel.");

                    BoardDatabase.AddToBlackList(ctx.Guild.Id, boardChannel.Id, channel.Id);
                    await ctx.EditResponseAsync(Util.EmbedReply(
                        $"{Constants.CheckEmoji} Channel {channel.Mention} has been added to the blacklist for board {boardChannel.Mention}."));

                    break;
                case BlacklistAction.Remove:
                    if (channel is null)
                        throw new CommandException("You need to provide a channel to remove.");
                    if (!BoardDatabase.TryGetBoard(b => b.GuildId == ctx.Guild.Id && b.Channel == boardChannel.Id,
                        out var board))
                        throw new CommandException("There currently is no board in the provided channel.");
                    if (!board.BlacklistedChannels.Contains(channel.Id))
                        throw new CommandException("The board doesn't have that channel blacklisted.");

                    BoardDatabase.RemoveFromBlacklist(ctx.Guild.Id, boardChannel.Id, channel.Id);
                    await ctx.EditResponseAsync(Util.EmbedReply(
                        $"{Constants.CheckEmoji} Channel {channel.Mention} has been removed from the blacklist for board {boardChannel.Mention}."));

                    break;
                case BlacklistAction.Clear:
                    if (!BoardDatabase.TryGetBoard(b => b.GuildId == ctx.Guild.Id && b.Channel == boardChannel.Id,
                        out _))
                        throw new CommandException("There currently is no board in the provided channel.");

                    var buttons = new[]
                    {
                        new DiscordButtonComponent(ButtonStyle.Success, "confirm_blacklist_clear", "Clear"),
                        new DiscordButtonComponent(ButtonStyle.Danger, "confirm_blacklist_abort", "Abort")
                    };

                    var m = await ctx.EditResponseAsync(Util.EmbedReply(
                            $"{Constants.InfoEmoji} Do you *really* want to clear all blacklisted channels for board {boardChannel.Mention}?")
                        // ReSharper disable once CoVariantArrayConversion
                        .AddComponents(buttons));

                    var x = await ctx.Client.GetInteractivity()
                        .WaitForButtonAsync(m, buttons);

                    if (x.Result.Id == "confirm_blacklist_clear")
                    {
                        BoardDatabase.ClearBlacklist(ctx.Guild.Id, boardChannel.Id);
                        await ctx.EditResponseAsync(
                            Util.EmbedReply($"{Constants.CheckEmoji} Blacklist has been cleared."));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(Util.EmbedReply($"{Constants.InfoEmoji} Clearing aborted."));
                    }

                    break;
                case BlacklistAction.List:
                    if (!BoardDatabase.TryGetBoard(b => b.GuildId == ctx.Guild.Id && b.Channel == boardChannel.Id,
                        out board))
                        throw new CommandException("There currently is no board in the provided channel.");

                    await ctx.EditResponseAsync(Util.EmbedReply($"{Constants.InfoEmoji} Blacklisted channels for {boardChannel.Mention}\n" +
                                                                string.Join("\n",
                                                                    board.BlacklistedChannels.Select(b => $"» <#{b}>"))));

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public enum BlacklistAction
        {
            [ChoiceName("add")] Add,
            [ChoiceName("remove")] Remove,
            [ChoiceName("clear")] Clear,
            [ChoiceName("list")] List
        }
    }
}