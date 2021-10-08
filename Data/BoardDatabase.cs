using System.Collections.Generic;
using LiteDB;

namespace MeiyounaiseSlash.Data
{
    public class BoardDatabase : BaseDatabase
    {

        #region POCOs

        public class Board
        {
            public string Id { get; set; }
            public ulong Channel { get; set; }
            public int AmountNeeded { get; set; }
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
    }
}