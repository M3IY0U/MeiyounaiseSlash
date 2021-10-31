using System.Collections.Generic;
using MeiyounaiseSlash.Data.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MeiyounaiseSlash.Data
{
    public class MeiyounaiseContext : DbContext
    {
        public DbSet<Scrobble> Scrobbles { get; set; }
        public DbSet<User> Users { get; set; }

        public MeiyounaiseContext(DbContextOptions<MeiyounaiseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scrobble>()
                .Property(p => p.Id)
                .IsConcurrencyToken()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>()
                .Property(u => u.NowPlayingReactions)
                .HasConversion(v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<HashSet<string>>(v));
            base.OnModelCreating(modelBuilder);
        }
    }
}