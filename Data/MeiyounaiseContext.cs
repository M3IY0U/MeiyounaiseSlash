using System.Collections.Generic;
using MeiyounaiseSlash.Data.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MeiyounaiseSlash.Data
{
    public class MeiyounaiseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Message> Messages { get; set; }

        public MeiyounaiseContext(DbContextOptions<MeiyounaiseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.NowPlayingReactions)
                .HasConversion(v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<HashSet<string>>(v));

            modelBuilder.Entity<Board>()
                .Property(b => b.BlacklistedChannels)
                .HasConversion(v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ulong>>(v));
            
            modelBuilder.Entity<Guild>()
                .Property(g => g.PinnedMessages)
                .HasConversion(v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<ulong, List<ulong>>>(v));

            base.OnModelCreating(modelBuilder);
        }
    }
}