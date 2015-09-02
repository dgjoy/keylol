using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics;
using Keylol.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.DAL
{
    public class KeylolDbContext : IdentityDbContext<KeylolUser>
    {
        public KeylolDbContext()
            : base("DefaultConnection", false)
        {
            Database.Log = s => Debug.WriteLine(s);
        }

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

        public DbSet<OfficialMessage> OfficialMessages { get; set; }
        public DbSet<OfficialMessageWithSender> OfficialMessagesWithSender { get; set; }

        public DbSet<CorrectionalServiceMessage> CorrectionalServiceMessages { get; set; }
        public DbSet<EditingMessage> EditingMessages { get; set; }
        public DbSet<SocialMessage> SocialMessages { get; set; }
        public DbSet<SystemMessage> SystemMessages { get; set; }

        public DbSet<WarningMessage> WarningMessages { get; set; }
        public DbSet<RejectionMessage> RejectionMessages { get; set; }
        public DbSet<ArchiveMessage> ArchiveMessages { get; set; }
        public DbSet<ArticleArchiveMessage> ArticleArchiveMessages { get; set; }
        public DbSet<CommentArchiveMessage> CommentArchiveMessages { get; set; }
        public DbSet<MuteMessage> MuteMessages { get; set; }
        public DbSet<RecommendationMessage> RecommendationMessages { get; set; }
        public DbSet<PointRecommendationMessage> PointRecommendationMessages { get; set; }
        public DbSet<GlobalRecommendationMessage> GlobalRecommendationMessages { get; set; }
        public DbSet<EditMessage> EditMessages { get; set; }
        public DbSet<LikeMessage> LikeMessages { get; set; }
        public DbSet<ArticleLikeMessage> ArticleLikeMessages { get; set; }
        public DbSet<CommentLikeMessage> CommentLikeMessages { get; set; }
        public DbSet<ReplyMessage> ReplayMessages { get; set; }
        public DbSet<ArticleReplyMessage> ArticleReplyMessages { get; set; }
        public DbSet<CommentReplyMessage> CommentReplyMessages { get; set; }
        public DbSet<AnnouncementMessage> AnnouncementMessages { get; set; }
        public DbSet<AdvertisementMessage> AdvertisementMessages { get; set; }

        public DbSet<Log> Logs { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<EditLog> EditLogs { get; set; }

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
                .WithMany(user => user.ManagedPoints)
                .Map(t => t.ToTable("PointStaffs"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(point => point.AssociatedToPoints)
                .WithMany(point => point.AssociatedByPoints)
                .Map(t => t.MapLeftKey("ToPoint_Id")
                    .MapRightKey("ByPoint_Id")
                    .ToTable("PointAssociations"));
            modelBuilder.Entity<Entry>()
                .HasMany(entry => entry.AttachedPoints)
                .WithMany(point => point.Entries)
                .Map(t => t.ToTable("EntryPointPushes"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(point => point.RecommendedArticles)
                .WithMany(article => article.RecommendedByPoints)
                .Map(t => t.ToTable("PointArticleRecommendation"));
            modelBuilder.Entity<Comment>()
                .HasMany(comment => comment.ReplyToComments)
                .WithMany(comment => comment.RepliedByComments)
                .Map(t => t.MapLeftKey("ByComment_Id")
                    .MapRightKey("ToComment_Id")
                    .ToTable("CommentReplies"));
            modelBuilder.Entity<LikeMessage>()
                .HasMany(message => message.Likes)
                .WithMany(like => like.RelatedLikeMessages)
                .Map(t => t.ToTable("LikeMessagePayload"));
        }

        public static KeylolDbContext Create()
        {
            return new KeylolDbContext();
        }
    }
}