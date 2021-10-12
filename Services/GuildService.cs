using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MeiyounaiseSlash.Data;

namespace MeiyounaiseSlash.Services
{
    public class GuildService
    {
        private GuildDatabase GuildDatabase { get; }
        private Dictionary<ulong, (DiscordMessage message, int count)> RepeatMessages { get; }

        public GuildService(GuildDatabase db)
        {
            GuildDatabase = db;
            RepeatMessages = new Dictionary<ulong, (DiscordMessage, int)>();
        }

        public async Task MemberJoined(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            var guild = GuildDatabase.GetOrCreateGuild(e.Guild.Id);
            if (guild.JoinChannel == 0 || string.IsNullOrEmpty(guild.JoinMessage))
                return;

            await e.Guild.GetChannel(guild.JoinChannel)
                .SendMessageAsync(guild.JoinMessage.Replace("[user]",
                    $"{e.Member.Mention}"));
        }

        public async Task MemberLeft(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            var guild = GuildDatabase.GetOrCreateGuild(e.Guild.Id);
            if (guild.LeaveChannel == 0 || string.IsNullOrEmpty(guild.LeaveMessage))
                return;

            await e.Guild.GetChannel(guild.LeaveChannel)
                .SendMessageAsync(guild.LeaveMessage.Replace("[user]",
                    $"{e.Member.Username}#{e.Member.Discriminator}"));
        }

        public async Task RepeatMessage(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;
            var guild = GuildDatabase.GetOrCreateGuild(e.Guild.Id);
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
    }
}