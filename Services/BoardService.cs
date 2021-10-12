using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MeiyounaiseSlash.Data;

namespace MeiyounaiseSlash.Services
{
    public class BoardService
    {
        private BoardDatabase BoardDatabase { get; }

        public BoardService(BoardDatabase db)
        {
            BoardDatabase = db;
        }

        public async Task ReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
        {
            if (args.User.IsBot) return;
            if (!BoardDatabase.TryGetBoard(b => b.GuildId == args.Guild.Id
                                                && !b.BlacklistedChannels.Contains(args.Channel.Id), out var board))
                return;

            var dbMsg = BoardDatabase.MessageCollection.FindOne(msg => msg.Id == args.Message.Id);
            var sourceMsg = await args.Channel.GetMessageAsync(args.Message.Id);

            if (dbMsg is null)
                BoardDatabase.MessageCollection.Insert(new BoardDatabase.Message
                {
                    Id = sourceMsg.Id,
                    HasBeenSent = false,
                    IdInBoard = 0
                });

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
            if (!BoardDatabase.TryGetBoard(b => b.GuildId == args.Guild.Id
                                                && !b.BlacklistedChannels.Contains(args.Channel.Id), out var board))
                return;

            var dbMsg = BoardDatabase.MessageCollection.FindOne(msg => msg.Id == args.Message.Id);

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
                    BoardDatabase.MessageCollection.Update(dbMsg);
                }
            }
        }

        private static string FormatReactions(DiscordMessage msg, long threshold)
            => string.Join(" \n ",
                (from reaction in msg.Reactions
                    where reaction.Count >= threshold
                    select (reaction, reaction.Count))
                .OrderByDescending(x => x.Count)
                .GroupBy(tuple => tuple.Count)
                .Select(grouping =>
                    $"**{grouping.Key}** × " +
                    $"{string.Join(" ", grouping.Select(x => x.reaction.Emoji))}"));

        private async void PostMessageToBoard(DiscordMessage msg, BoardDatabase.Board board, string reactions)
        {
            var embed = BoardEmbedFromMessage(msg, reactions);

            var boardMsg = await msg.Channel.Guild.GetChannel(board.ChannelId)
                .SendMessageAsync(new DiscordMessageBuilder()
                    .WithEmbed(embed));

            var dbMsg = BoardDatabase.MessageCollection.FindOne(x => x.Id == msg.Id);
            dbMsg.HasBeenSent = true;
            dbMsg.IdInBoard = boardMsg.Id;
            BoardDatabase.MessageCollection.Update(dbMsg);
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