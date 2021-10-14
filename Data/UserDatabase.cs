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
    }
}