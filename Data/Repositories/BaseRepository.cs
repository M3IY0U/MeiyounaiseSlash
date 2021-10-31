using Microsoft.EntityFrameworkCore;

namespace MeiyounaiseSlash.Data.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly MeiyounaiseContext Context;
        protected DbSet<T> Entities;

        protected BaseRepository(MeiyounaiseContext ctx)
        {
            Context = ctx;
            Entities = ctx.Set<T>();
        }
    }
}