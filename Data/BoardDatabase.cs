using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;

namespace MeiyounaiseSlash.Data
{
    public class BoardDatabase : BaseDatabase
    {
        #region POCOs

        public class Board
        {
            public int Id { get; set; }
            public ulong GuildId { get; set; }
            public ulong ChannelId { get; set; }
            public long Threshold { get; set; }
            public List<ulong> BlacklistedChannels { get; set; }
        }

        public class Message
        {
            public ulong Id { get; set; }
            public ulong IdInBoard { get; set; }
            public bool HasBeenSent { get; set; }
        }

        #endregion

        private ILiteCollection<Board> _boardCollection;
        public ILiteCollection<Message> MessageCollection;

        public BoardDatabase(string path) : base(path)
        {
            _boardCollection = Database.GetCollection<Board>("boards");
            MessageCollection = Database.GetCollection<Message>("messages");
        }

        public bool TryGetBoard(Expression<Func<Board, bool>> predicate, out Board board)
        {
            board = _boardCollection.FindOne(predicate);
            return board is not null;
        }

        public bool UpsertBoard(ulong guildId, ulong channel, long amount)
        {
            if (!TryGetBoard(b => b.GuildId == guildId, out var board))
            {
                _boardCollection.Insert(new Board
                {
                    ChannelId = channel,
                    GuildId = guildId,
                    Threshold = amount,
                    BlacklistedChannels = new List<ulong>()
                });
                return true;
            }

            board.ChannelId = channel;
            board.Threshold = amount;
            _boardCollection.Update(board);
            return false;
        }

        public void DeleteBoard(ulong guildId, ulong channelId)
            => _boardCollection.DeleteMany(board => board.GuildId == guildId && board.ChannelId == channelId);

        public void AddToBlackList(ulong guildId, ulong channelToAdd)
        {
            var toUpdate = _boardCollection.FindOne(board => board.GuildId == guildId);
            toUpdate.BlacklistedChannels.Add(channelToAdd);
            _boardCollection.Update(toUpdate);
        }

        public void RemoveFromBlacklist(ulong guildId, ulong channelToRemove)
        {
            var toUpdate = _boardCollection.FindOne(board => board.GuildId == guildId);
            toUpdate.BlacklistedChannels.Remove(channelToRemove);
            _boardCollection.Update(toUpdate);
        }

        public void ClearBlacklist(ulong guildId)
        {
            var toUpdate = _boardCollection.FindOne(board => board.GuildId == guildId);
            toUpdate.BlacklistedChannels.Clear();
            _boardCollection.Update(toUpdate);
        }
    }
}