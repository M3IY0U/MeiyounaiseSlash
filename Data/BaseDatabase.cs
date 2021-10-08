using LiteDB;

namespace MeiyounaiseSlash.Data
{
    public abstract class BaseDatabase
    {
        protected readonly LiteDatabase Database;
        
        protected BaseDatabase(string path) => Database = new LiteDatabase(path);
    }
}