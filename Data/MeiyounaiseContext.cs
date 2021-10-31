using MeiyounaiseSlash.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MeiyounaiseSlash.Data
{
    public class MeiyounaiseContext : DbContext
    {
        public DbSet<Scrobble> Scrobbles { get; set; }

        public MeiyounaiseContext(DbContextOptions<MeiyounaiseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scrobble>()
                .Property(p => p.Id)
                .IsConcurrencyToken()
                .ValueGeneratedOnAdd();
            base.OnModelCreating(modelBuilder);
        }
    }
}