using LiteDB;

namespace MeiyounaiseSlash.Data
{
    public class UserDatabase : BaseDatabase
    {
        #region POCOs

        public class User
        {
            public ulong Id { get; init; }
            public string LastFm { get; set; }
        }

        #endregion

        private ILiteCollection<User> _userCollection; 
        
        public UserDatabase(string path) : base(path)
        {
            _userCollection = Database.GetCollection<User>("users");
        }
    }
}