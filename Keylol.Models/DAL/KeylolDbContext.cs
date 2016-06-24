using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Models.DAL
{
    public class KeylolDbContext : IdentityDbContext<KeylolUser>
    {
        public enum ConcurrencyStrategy
        {
            ClientWin,
            DatabaseWin
        }

        public KeylolDbContext() : base("DefaultConnection", false)
        {
        }
        
        public DbSet<Point> Points { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleComment> ArticleComments { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<SteamBindingToken> SteamBindingTokens { get; set; }
        public DbSet<SteamBot> SteamBots { get; set; }
        public DbSet<UserSteamGameRecord> UserSteamGameRecords { get; set; }
        public DbSet<SteamStoreName> SteamStoreNames { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<CouponLog> CouponLogs { get; set; }
        public DbSet<CouponGift> CouponGifts { get; set; }
        public DbSet<CouponGiftOrder> CouponGiftOrders { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityComment> ActivityComments { get; set; }
        public DbSet<AtRecord> AtRecords { get; set; }
        public DbSet<Conference> Conferences { get; set; }
        public DbSet<ConferenceEntry> ConferenceEntries { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PointRelationship> PointRelationships { get; set; }
        public DbSet<Feed> Feeds { get; set; }
        public DbSet<UserSteamFriendRecord> UserSteamFriendRecords { get; set; }
        public DbSet<PointStaff> PointStaff{ get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Entity<KeylolUser>().ToTable("KeylolUsers");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            
            modelBuilder.Entity<Point>()
                .HasMany(p => p.SteamStoreNames)
                .WithMany(n => n.Points)
                .Map(t => t.ToTable("PointStoreNameMappings"));
        }

        public async Task<int> SaveChangesAsync(ConcurrencyStrategy concurrencyStrategy)
        {
            do
            {
                try
                {
                    return await SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    switch (concurrencyStrategy)
                    {
                        case ConcurrencyStrategy.ClientWin:
                            var entry = e.Entries.Single();
                            entry.OriginalValues.SetValues(await entry.GetDatabaseValuesAsync());
                            break;

                        case ConcurrencyStrategy.DatabaseWin:
                            await e.Entries.Single().ReloadAsync();
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(concurrencyStrategy), concurrencyStrategy, null);
                    }
                }
            } while (true);
        }
    }
}