using System.Collections.Generic;
using LiteDB;

namespace MeiyounaiseSlash.Data
{
    public class UserDatabase : BaseDatabase
    {
        #region POCOs

        private class User
        {
            public ulong Id { get; init; }
            public string LastFm { get; set; }
            public HashSet<ulong> NowPlayingReactions { get; set; } = new ();
        }

        #endregion

        private readonly ILiteCollection<User> _userCollection;

        public UserDatabase(string path) : base(path) => _userCollection = Database.GetCollection<User>("users");

        public bool TryGetLast(ulong user, out string last)
        {
            last = _userCollection.FindOne(x => x.Id == user)?.LastFm;
            return !string.IsNullOrEmpty(last);
        }

        public void SetLastAccount(ulong id, string last)
            => _userCollection.Upsert(new User
            {
                Id = id,
                LastFm = last
            });

        public void AddReaction(ulong userId, ulong reaction)
        {
            var user = _userCollection.FindOne(u => u.Id == userId);
            if (user is null) return;
            user.NowPlayingReactions ??= new HashSet<ulong>();
            user.NowPlayingReactions.Add(reaction);
            _userCollection.Update(user);
        }

        public void ClearReactions(ulong userId)
        {
            var user = _userCollection.FindOne(u => u.Id == userId);
            if (user is null) return;
            user.NowPlayingReactions ??= new HashSet<ulong>();
            user.NowPlayingReactions.Clear();
            _userCollection.Update(user);
        }

        public HashSet<ulong> GetReactions(ulong userId)
        {
            var user = _userCollection.FindOne(u => u.Id == userId);
            if (user is null) return null;
            if (user.NowPlayingReactions != null) return user.NowPlayingReactions;
            user.NowPlayingReactions = new HashSet<ulong>();
            _userCollection.Update(user);
            return user.NowPlayingReactions;
        }
    }
}