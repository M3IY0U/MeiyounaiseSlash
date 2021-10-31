using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MeiyounaiseSlash.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MeiyounaiseSlash.Data.Repositories
{
    public class BoardRepository : BaseRepository<Board>
    {
        private readonly DbSet<Message> _messages;

        public BoardRepository(MeiyounaiseContext ctx) : base(ctx)
        {
            _messages = ctx.Set<Message>();
        }

        public bool TryGetBoard(Expression<Func<Board, bool>> predicate, out Board board)
        {
            board = Entities.AsQueryable().SingleOrDefault(predicate);
            return board is not null;
        }

        public bool UpsertBoard(ulong guildId, ulong channel, long amount)
        {
            if (!TryGetBoard(b => b.GuildId == guildId, out var board))
            {
                Entities.Add(new Board
                {
                    ChannelId = channel,
                    GuildId = guildId,
                    Threshold = amount,
                    BlacklistedChannels = new List<ulong>()
                });
                Context.SaveChanges();
                return true;
            }

            board.ChannelId = channel;
            board.Threshold = amount;
            Context.Update(board);
            Context.SaveChanges();
            return false;
        }

        public void DeleteBoard(ulong guildId, ulong channelId)
        {
            var toRemove = Entities.SingleOrDefault(b => b.GuildId == guildId && b.ChannelId == channelId);
            if (toRemove is null) return;
            Context.Entry(toRemove).State = EntityState.Deleted;
            Context.SaveChanges();
        }

        public void AddToBlackList(ulong guildId, ulong channelToAdd)
        {
            var toUpdate = Entities.Single(board => board.GuildId == guildId);
            toUpdate.BlacklistedChannels.Add(channelToAdd);
            Entities.Update(toUpdate);
            Context.SaveChanges();
        }

        public void RemoveFromBlacklist(ulong guildId, ulong channelToRemove)
        {
            var toUpdate = Entities.Single(board => board.GuildId == guildId);
            toUpdate.BlacklistedChannels.Remove(channelToRemove);
            Entities.Update(toUpdate);
            Context.SaveChanges();
        }

        public void ClearBlacklist(ulong guildId)
        {
            var toUpdate = Entities.Single(board => board.GuildId == guildId);
            toUpdate.BlacklistedChannels.Clear();
            Entities.Update(toUpdate);
            Context.SaveChanges();
        }

        public void InsertMessage(ulong id)
        {
            _messages.Add(new Message
            {
                Id = id,
                HasBeenSent = false,
                IdInBoard = 0
            });
            Context.SaveChanges();
        }

        public Message GetMessage(ulong id)
        {
            return _messages.SingleOrDefault(m => m.Id == id);
        }

        public void UpdateMessage(Message msg)
        {
            _messages.Update(msg);
            Context.SaveChanges();
        }
    }
}