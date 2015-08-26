using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Keylol.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.DAL
{
    public class KeylolDbContext : IdentityDbContext<KeylolUser>
    {
        public KeylolDbContext()
            : base("DefaultConnection", false) {}

        public DbSet<Point> Points { get; set; }
        public DbSet<NormalPoint> NormalPoints { get; set; }
        public DbSet<ProfilePoint> ProfilePoints { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleType> ArticleTypes { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<ArticleLike> ArticleLikes { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<SystemMessage> SystemMessages { get; set; }
        public DbSet<SystemMessageLikeNotification> SystemMessageLikeNotifications { get; set; }
        public DbSet<SystemMessageReplyNotification> SystemMessageReplyNotifications { get; set; }
        public DbSet<SystemMessageWarningNotification> SystemMessageWarningNotifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Remove cascade delete conventions because we only use soft delete in this website
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Entity<KeylolUser>().ToTable("KeylolUsers");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");

            modelBuilder.Entity<KeylolUser>()
                .HasMany(user => user.SubscribedPoints)
                .WithMany(point => point.Subscribers)
                .Map(t => t.ToTable("UserPointSubscriptions"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(point => point.Staffs)
                .WithMany(user => user.ModeratedPoints)
                .Map(t => t.ToTable("PointModerators"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(point => point.AssociatedToPoints)
                .WithMany(point => point.AssociatedByPoints)
                .Map(t => t.ToTable("PointAssociations"));
            modelBuilder.Entity<Entry>()
                .HasMany(entry => entry.AttachedPoints)
                .WithMany(point => point.Entries)
                .Map(t => t.ToTable("EntryPointPushes"));
            modelBuilder.Entity<Comment>()
                .HasMany(comment => comment.ReplyToComments)
                .WithMany(comment => comment.RepliedByComments)
                .Map(t => t.MapLeftKey("ByComment_Id")
                    .MapRightKey("ToComment_Id")
                    .ToTable("CommentReplies"));
        }

        public static KeylolDbContext Create()
        {
            return new KeylolDbContext();
        }
    }
}