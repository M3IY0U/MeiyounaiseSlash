using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MeiyounaiseSlash.Data.Models;
using MeiyounaiseSlash.Data.Repositories;

namespace MeiyounaiseSlash.Services
{
    public class BoardService
    {
        private BoardRepository BoardRepository { get; }

        public BoardService(BoardRepository repo)
        {
            BoardRepository = repo;
        }

        public async Task ReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
        {
            if (args.User.IsBot) return;
            if (!BoardRepository.TryGetBoard(b => b.GuildId == args.Guild.Id, out var board))
                return;
            if (board.BlacklistedChannels.Contains(args.Channel.Id))
                return;

            var dbMsg = BoardRepository.GetMessage(args.Message.Id);
            var sourceMsg = await args.Channel.GetMessageAsync(args.Message.Id);

            if (dbMsg is null)
            {
                BoardRepository.InsertMessage(sourceMsg.Id);
            }

            if (dbMsg is not null && dbMsg.HasBeenSent)
            {
                var msgInBoard = await args.Guild.GetChannel(board.ChannelId).GetMessageAsync(dbMsg.IdInBoard);

                await msgInBoard.ModifyAsync(BoardEmbedFromMessage(sourceMsg,
                    FormatReactions(sourceMsg, board.Threshold)));
            }
            else
            {
                if (!sourceMsg.Reactions.Any(r => r.Count >= board.Threshold))
                    return;

                PostMessageToBoard(sourceMsg, board, FormatReactions(sourceMsg, board.Threshold));
            }
        }

        public async Task ReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs args)
        {
            if (args.User.IsBot) return;
            if (!BoardRepository.TryGetBoard(b => b.GuildId == args.Guild.Id, out var board))
                return;
            if (board.BlacklistedChannels.Contains(args.Channel.Id))
                return;

            var dbMsg = BoardRepository.GetMessage(args.Message.Id);

            if (dbMsg is not null && dbMsg.HasBeenSent)
            {
                var sourceMsg = await args.Channel.GetMessageAsync(args.Message.Id);
                var msgInBoard = await args.Guild.GetChannel(board.ChannelId).GetMessageAsync(dbMsg.IdInBoard);
                if (sourceMsg.Reactions.Any(r => r.Count >= board.Threshold))
                {
                    await msgInBoard.ModifyAsync(BoardEmbedFromMessage(sourceMsg,
                        FormatReactions(sourceMsg, board.Threshold)));
                }
                else
                {
                    await msgInBoard.DeleteAsync();
                    dbMsg.HasBeenSent = false;
                    dbMsg.IdInBoard = 0;
                    BoardRepository.UpdateMessage(dbMsg);
                }
            }
        }

        private static string FormatReactions(DiscordMessage msg, long threshold)
            => string.Join(" \n ",
                msg.Reactions
                    .Where(reaction => reaction.Count >= threshold)
                    .Select(reaction => (reaction, reaction.Count))
                    .OrderByDescending(tuple => tuple.Count)
                    .GroupBy(tuple => tuple.Count)
                    .Select(grouping =>
                        $"**{grouping.Key}** × " +
                        $"{string.Join(" ", grouping.Select(x => x.reaction.Emoji))}"));

        private async void PostMessageToBoard(DiscordMessage msg, Board board, string reactions)
        {
            var embed = BoardEmbedFromMessage(msg, reactions);

            var boardMsg = await msg.Channel.Guild.GetChannel(board.ChannelId)
                .SendMessageAsync(new DiscordMessageBuilder()
                    .WithEmbed(embed));

            var dbMsg = BoardRepository.GetMessage(msg.Id);
            dbMsg.HasBeenSent = true;
            dbMsg.IdInBoard = boardMsg.Id;
            BoardRepository.UpdateMessage(dbMsg);
        }

        private static DiscordEmbed BoardEmbedFromMessage(DiscordMessage msg, string reactions)
        {
            var member = (DiscordMember) msg.Author;
            var content = new List<string> {msg.Content[.. Math.Min(msg.Content.Length, 3920)]};
            var imageUrl = string.Empty;

            if (msg.Embeds.Any())
            {
                imageUrl = msg.Embeds
                    .Where(e => e.Thumbnail is not null || e.Image is not null)
                    .Select(e => e.Thumbnail is not null ? e.Thumbnail.Url : e.Image.Url)
                    .First().ToString();
                if (string.IsNullOrEmpty(content[0]))
                    if (!string.IsNullOrEmpty(msg.Embeds[0].Description))
                        content.Add(msg.Embeds[0].Description);
            }
            else if (msg.Attachments.Any())
            {
                imageUrl = msg.Attachments[0].Url;
                content.Add(
                    $"📎 {Formatter.MaskedUrl(msg.Attachments[0].FileName, new Uri(msg.Attachments[0].ProxyUrl))}");
            }

            var embed = new DiscordEmbedBuilder()
                .WithAuthor($"Message by {member.DisplayName}")
                .WithThumbnail(member.AvatarUrl)
                .AddField("Reactions", reactions, true)
                .AddField("Posted in",
                    msg.Channel.Type == ChannelType.PublicThread
                        ? $"{(msg.Channel as DiscordThreadChannel)?.Parent.Mention}🧵{msg.Channel.Mention}"
                        : $"{msg.Channel.Mention}", true)
                .AddField("Link", Formatter.MaskedUrl("Jump to message", msg.JumpLink), true)
                .WithColor(member.Color)
                .WithTimestamp(msg.Timestamp);

            if (content.Count > 0)
                embed.WithDescription(string.Join("\n", content));

            if (!string.IsNullOrEmpty(imageUrl))
                embed.WithImageUrl(imageUrl);

            return embed;
        }
    }
}