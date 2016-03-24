using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics;
using System.Threading.Tasks;
using Keylol.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Models.DAL
{
    public class KeylolDbContext : IdentityDbContext<KeylolUser>
    {
        public static Action<string> LogAction;

        public KeylolDbContext() : base("DefaultConnection", false)
        {
            Database.Log = LogAction;
        }

        public DbSet<Point> Points { get; set; }
        public DbSet<NormalPoint> NormalPoints { get; set; }
        public DbSet<ProfilePoint> ProfilePoints { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentReply> CommentReplies { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<ArticleLike> ArticleLikes { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<EditLog> EditLogs { get; set; }
        public DbSet<SteamBindingToken> SteamBindingTokens { get; set; }
        public DbSet<SteamLoginToken> SteamLoginTokens { get; set; }
        public DbSet<SteamBot> SteamBots { get; set; }
        public DbSet<InvitationCode> InvitationCodes { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<AutoSubscription> AutoSubscriptions { get; set; }
        public DbSet<UserGameRecord> UserGameRecords { get; set; }
        public DbSet<SteamStoreName> SteamStoreNames { get; set; }

        #region Messages

        public DbSet<Message> Messages { get; set; }
        public DbSet<LikeMessage> LikeMessages { get; set; }
        public DbSet<ArticleLikeMessage> ArticleLikeMessages { get; set; }
        public DbSet<CommentLikeMessage> CommentLikeMessages { get; set; }
        public DbSet<CommentMessage> CommentMessages { get; set; }
        public DbSet<ArticleCommentMessage> ArticleCommentMessages { get; set; }
        public DbSet<CommentReplyMessage> CommentReplyMessages { get; set; }
        public DbSet<MissiveMessage> MissiveMessages { get; set; }
        public DbSet<ArticleArchiveMissiveMessage> ArticleArchiveMissiveMessages { get; set; }
        public DbSet<ArticleArchiveCancelMissiveMessage> ArticleArchiveCancelMissiveMessages { get; set; }
        public DbSet<CommentArchiveMissiveMessage> CommentArchiveMissiveMessages { get; set; }
        public DbSet<CommentArchiveCancelMissiveMessage> CommentArchiveCancelMissiveMessages { get; set; }
        public DbSet<RejectionMissiveMessage> RejectionMissiveMessages { get; set; }
        public DbSet<RejectionCancelMissiveMessage> RejectionCancelMissiveMessages { get; set; }
        public DbSet<SpotlightMissiveMessage> SpotlightMissiveMessages { get; set; }
        public DbSet<SpotlightCancelMissiveMessage> SpotlightCancelMissiveMessages { get; set; }
        public DbSet<ArticleWarningMissiveMessage> ArticleWarningMissiveMessages { get; set; }
        public DbSet<ArticleWarningCancelMissiveMessage> ArticleWarningCancelMissiveMessages { get; set; }
        public DbSet<CommentWarningMissiveMessage> CommentWarningMissiveMessages { get; set; }
        public DbSet<CommentWarningCancelMissiveMessage> CommentWarningCancelMissiveMessages { get; set; }

        #endregion

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

            modelBuilder.Entity<ProfilePoint>().Map(t => t.MapInheritedProperties().ToTable("ProfilePoints"));
            modelBuilder.Entity<NormalPoint>().Map(t => t.MapInheritedProperties().ToTable("NormalPoints"));

            modelBuilder.Entity<KeylolUser>()
                .HasMany(user => user.SubscribedPoints)
                .WithMany(point => point.Subscribers)
                .Map(t => t.ToTable("UserPointSubscriptions"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(point => point.Staffs)
                .WithMany(user => user.ManagedPoints)
                .Map(t => t.ToTable("PointStaffs"));
            modelBuilder.Entity<Article>()
                .HasMany(article => article.AttachedPoints)
                .WithMany(point => point.Articles)
                .Map(t => t.ToTable("ArticlePointPushes"));
            modelBuilder.Entity<Comment>()
                .HasMany(comment => comment.CommentRepliesAsComment)
                .WithRequired(reply => reply.Comment);
            modelBuilder.Entity<Comment>()
                .HasMany(comment => comment.CommentRepliesAsReply)
                .WithRequired(reply => reply.Reply);
            modelBuilder.Entity<InvitationCode>()
                .HasOptional(c => c.UsedByUser)
                .WithOptionalDependent(c => c.InvitationCode);
            modelBuilder.Entity<NormalPoint>()
                .HasMany(p => p.DeveloperPoints)
                .WithMany(p => p.DeveloperForPoints)
                .Map(t => t.MapLeftKey("GamePoint_Id")
                    .MapRightKey("DeveloperPoint_Id")
                    .ToTable("GameDeveloperPointAssociations"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(p => p.PublisherPoints)
                .WithMany(p => p.PublisherForPoints)
                .Map(t => t.MapLeftKey("GamePoint_Id")
                    .MapRightKey("PublisherPoint_Id")
                    .ToTable("GamePublisherPointAssociations"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(p => p.GenrePoints)
                .WithMany(p => p.GenreForPoints)
                .Map(t => t.MapLeftKey("GamePoint_Id")
                    .MapRightKey("GenrePoint_Id")
                    .ToTable("GameGenrePointAssociations"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(p => p.TagPoints)
                .WithMany(p => p.TagForPoints)
                .Map(t => t.MapLeftKey("GamePoint_Id")
                    .MapRightKey("TagPoint_Id")
                    .ToTable("GameTagPointAssociations"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(p => p.MajorPlatformPoints)
                .WithMany(p => p.MajorPlatformForPoints)
                .Map(t => t.MapLeftKey("GamePoint_Id")
                    .MapRightKey("MajorPlatformPoint_Id")
                    .ToTable("GameMajorPlatformPointAssociations"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(p => p.MinorPlatformPoints)
                .WithMany(p => p.MinorPlatformForPoints)
                .Map(t => t.MapLeftKey("GamePoint_Id")
                    .MapRightKey("MinorPlatformPoint_Id")
                    .ToTable("GameMinorPlatformPointAssociations"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(p => p.SeriesPoints)
                .WithMany(p => p.SeriesForPoints)
                .Map(t => t.MapLeftKey("GamePoint_Id")
                    .MapRightKey("SeriesPoint_Id")
                    .ToTable("GameSeriesPointAssociations"));
            modelBuilder.Entity<NormalPoint>()
                .HasMany(p => p.SteamStoreNames)
                .WithMany(n => n.NormalPoints)
                .Map(t => t.ToTable("PointStoreNameMappings"));
        }

        // Ignore validation error on unmodified properties
//        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry,
//            IDictionary<object, object> items)
//        {
//            var result = base.ValidateEntity(entityEntry, items);
//            var falseErrors = result.ValidationErrors
//                .Where(error =>
//                {
//                    var member = entityEntry.Member(error.PropertyName);
//                    var property = member as DbPropertyEntry;
//                    if (property != null)
//                        return !property.IsModified;
//                    return false;
//                });
//
//            foreach (var error in falseErrors.ToArray())
//                result.ValidationErrors.Remove(error);
//
//            return result;
//        }

        public static KeylolDbContext Create()
        {
            return new KeylolDbContext();
        }

        public async Task GiveNextSequenceNumberAsync<TEntity>(TEntity entity) where TEntity : IHaveSequenceNumber
        {
            entity.SequenceNumber =
                await Database.SqlQuery<int>($"SELECT NEXT VALUE FOR {entity.SequenceName}").SingleAsync();
        }
    }

    public interface IHaveSequenceNumber
    {
        string SequenceName { get; }
        int SequenceNumber { get; set; }
    }
}