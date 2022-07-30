using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MeiyounaiseSlash.Data.Repositories;

namespace MeiyounaiseSlash.Services
{
    public class GuildService
    {
        private GuildRepository GuildRepository { get; }
        private Dictionary<ulong, (DiscordMessage message, int count)> RepeatMessages { get; }

        public GuildService(GuildRepository db)
        {
            GuildRepository = db;
            RepeatMessages = new Dictionary<ulong, (DiscordMessage, int)>();
        }

        public async Task MemberJoined(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            var guild = await GuildRepository.GetOrCreateGuild(e.Guild.Id);
            if (guild.JoinChannel == 0 || string.IsNullOrEmpty(guild.JoinMessage))
                return;

            await e.Guild.GetChannel(guild.JoinChannel)
                .SendMessageAsync(guild.JoinMessage.Replace("[user]",
                    $"{e.Member.Mention}"));
        }

        public async Task MemberLeft(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            var guild = await GuildRepository.GetOrCreateGuild(e.Guild.Id);
            if (guild.LeaveChannel == 0 || string.IsNullOrEmpty(guild.LeaveMessage))
                return;

            await e.Guild.GetChannel(guild.LeaveChannel)
                .SendMessageAsync(guild.LeaveMessage.Replace("[user]",
                    $"{e.Member.Username}#{e.Member.Discriminator}"));
        }

        public async Task RepeatMessage(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;
            var guild = await GuildRepository.GetOrCreateGuild(e.Guild.Id);
            if (guild.RepeatMessages == 0) return;

            // add channel 
            if (!RepeatMessages.ContainsKey(e.Channel.Id))
            {
                RepeatMessages.TryAdd(e.Channel.Id, (e.Message, 1));
                return;
            }
            
            // i forgor why i did this but it works 💀
            if (RepeatMessages[e.Channel.Id].message == null)
            {
                RepeatMessages[e.Channel.Id] = (e.Message, 1);
                return;
            }

            // if author is different & message content/attachment is the same, increase count (or reset it)
            if (RepeatMessages[e.Channel.Id].message.Content == e.Message.Content &&
                RepeatMessages[e.Channel.Id].message.Author != e.Message.Author)
            {
                var count = RepeatMessages[e.Channel.Id].count;
                RepeatMessages[e.Channel.Id] = (e.Message, count + 1);
            }
            else
            {
                RepeatMessages[e.Channel.Id] = (e.Message, 1);
            }
            
            // if count is high enough, send the message and reset it
            if (RepeatMessages[e.Channel.Id].count >= guild.RepeatMessages)
            {
                if (string.IsNullOrEmpty(e.Message.Content))
                    await e.Channel.SendMessageAsync(e.Message.Attachments?[0].Url);
                else
                    await e.Channel.SendMessageAsync(e.Message.Content);

                RepeatMessages[e.Channel.Id] = (null, 1);
            }
        }

        public async Task ChannelPinsUpdated(DiscordClient sender, ChannelPinsUpdateEventArgs args)
        {
            var guild = await GuildRepository.GetOrCreateGuild(args.Guild.Id);
            if (guild.PinArchiveChannel == 0)
                return;

            var oldPinned = guild.PinnedMessages[args.Channel.Id];
            var currentPinned = (await args.Channel.GetPinnedMessagesAsync()).Select(c => c.Id).ToList();

            if (currentPinned.Count < oldPinned.Count) // only act if something got unpinned
            {
                var missing = oldPinned.Except(currentPinned);
                var oldMsg = await args.Channel.GetMessageAsync(missing.First());

                var eb = new DiscordEmbedBuilder()
                    .WithAuthor($"📌 Moved old pin from {((DiscordMember)oldMsg.Author).DisplayName}",
                        iconUrl: oldMsg.Author.AvatarUrl)
                    .AddField("Channel", $"{oldMsg.Channel.Mention}", true)
                    .AddField("Jump to", Formatter.MaskedUrl("Message", oldMsg.JumpLink), true);

                var imageUrl = string.Empty;

                if (oldMsg.Embeds.Any())
                {
                    imageUrl = oldMsg.Embeds
                        .Where(e => e.Thumbnail is not null || e.Image is not null)
                        .Select(e => e.Thumbnail is not null ? e.Thumbnail.Url : e.Image.Url)
                        .FirstOrDefault()?.ToString();
                    if (string.IsNullOrEmpty(eb.Description))
                        if (!string.IsNullOrEmpty(oldMsg.Embeds[0].Description))
                            eb.Description += oldMsg.Embeds[0].Description;
                }
                else if (oldMsg.Attachments.Any())
                {
                    imageUrl = oldMsg.Attachments[0].Url;
                    eb.Description +=
                        $"📎 {Formatter.MaskedUrl(oldMsg.Attachments[0].FileName, new Uri(oldMsg.Attachments[0].ProxyUrl))}";
                }

                if (!string.IsNullOrEmpty(imageUrl))
                    eb.WithImageUrl(imageUrl);
                
                if (string.IsNullOrEmpty(eb.Description))
                    eb.WithDescription(oldMsg.Content.Length > 2048 ? oldMsg.Content[.. 2048] + " [...]" : oldMsg.Content);
                
                await args.Guild.GetChannel(guild.PinArchiveChannel).SendMessageAsync(eb);
            }

            await GuildRepository.UpdateArchive(args.Guild.Id, args.Channel.Id, currentPinned);
        }

        public static async Task<Dictionary<ulong, List<ulong>>> GetPinnedMessagesInGuild(DiscordGuild guild)
        {
            var channels = new Dictionary<ulong, List<ulong>>();
            foreach (var chn in guild.Channels.Values.Where(c =>
                         c.Type is ChannelType.Text or ChannelType.PublicThread))
            {
                var messages = await chn.GetPinnedMessagesAsync();
                channels.Add(chn.Id, messages.Select(m => m.Id).ToList());
            }

            return channels;
        }
    }
}