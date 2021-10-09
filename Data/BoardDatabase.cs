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
            public ulong Channel { get; set; }
            public long AmountNeeded { get; set; }
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
        private ILiteCollection<Message> _messageCollection;

        public BoardDatabase(string path) : base(path)
        {
            _boardCollection = Database.GetCollection<Board>("boards");
            _messageCollection = Database.GetCollection<Message>("messages");
        }

        public bool TryGetBoard(Expression<Func<Board, bool>> predicate, out Board board)
        {
            board = _boardCollection.FindOne(predicate);
            return board is not null;
        }
    
        public void AddBoard(ulong guildId, ulong channel, long amount)
            => _boardCollection.Insert(new Board
            {
                Channel = channel,
                GuildId = guildId,
                AmountNeeded = amount,
                BlacklistedChannels = new List<ulong>()
            });

        public List<Board> GetBoardsInGuild(ulong guildId)
            => _boardCollection.Query().Where(board => board.GuildId == guildId).ToList();

        public void DeleteBoard(ulong guildId, ulong channelId)
            => _boardCollection.DeleteMany(board => board.GuildId == guildId && board.Channel == channelId);

        public void AddToBlackList(ulong guildId, ulong channelId, ulong channelToAdd)
        {
            var toUpdate = _boardCollection.FindOne(board => board.GuildId == guildId && board.Channel == channelId);
            toUpdate.BlacklistedChannels.Add(channelToAdd);
            _boardCollection.Update(toUpdate);
        }

        public void RemoveFromBlacklist(ulong guildId, ulong channelId, ulong channelToRemove)
        {
            var toUpdate = _boardCollection.FindOne(board => board.GuildId == guildId && board.Channel == channelId);
            toUpdate.BlacklistedChannels.Remove(channelToRemove);
            _boardCollection.Update(toUpdate);
        }

        public void ClearBlacklist(ulong guildId, ulong channelId)
        {
            var toUpdate = _boardCollection.FindOne(board => board.GuildId == guildId && board.Channel == channelId);
            toUpdate.BlacklistedChannels.Clear();
            _boardCollection.Update(toUpdate);
        }
    }
}