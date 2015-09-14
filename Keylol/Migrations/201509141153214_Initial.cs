namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Time = c.DateTime(nullable: false),
                        Read = c.Boolean(nullable: false),
                        Type = c.Int(),
                        Type1 = c.Int(),
                        Content = c.String(),
                        Content1 = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Point_Id = c.String(maxLength: 128),
                        Article_Id = c.String(maxLength: 128),
                        Comment_Id = c.String(maxLength: 128),
                        Article_Id1 = c.String(maxLength: 128),
                        Article_Id2 = c.String(maxLength: 128),
                        Article_Id3 = c.String(maxLength: 128),
                        Article_Id4 = c.String(maxLength: 128),
                        Article_Id5 = c.String(maxLength: 128),
                        Target_Id = c.String(maxLength: 128),
                        Comment_Id1 = c.String(maxLength: 128),
                        Comment_Id2 = c.String(maxLength: 128),
                        Target_Id1 = c.String(maxLength: 128),
                        Receiver_Id = c.String(nullable: false, maxLength: 128),
                        Sender_Id = c.String(maxLength: 128),
                        Sender_Id1 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NormalPoints", t => t.Point_Id)
                .ForeignKey("dbo.Entries", t => t.Article_Id)
                .ForeignKey("dbo.Comments", t => t.Comment_Id)
                .ForeignKey("dbo.Entries", t => t.Article_Id1)
                .ForeignKey("dbo.Entries", t => t.Article_Id2)
                .ForeignKey("dbo.Entries", t => t.Article_Id3)
                .ForeignKey("dbo.Entries", t => t.Article_Id4)
                .ForeignKey("dbo.Entries", t => t.Article_Id5)
                .ForeignKey("dbo.Entries", t => t.Target_Id)
                .ForeignKey("dbo.Comments", t => t.Comment_Id1)
                .ForeignKey("dbo.Comments", t => t.Comment_Id2)
                .ForeignKey("dbo.Comments", t => t.Target_Id1)
                .ForeignKey("dbo.KeylolUsers", t => t.Receiver_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Sender_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Sender_Id1)
                .Index(t => t.Point_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.Comment_Id)
                .Index(t => t.Article_Id1)
                .Index(t => t.Article_Id2)
                .Index(t => t.Article_Id3)
                .Index(t => t.Article_Id4)
                .Index(t => t.Article_Id5)
                .Index(t => t.Target_Id)
                .Index(t => t.Comment_Id1)
                .Index(t => t.Comment_Id2)
                .Index(t => t.Target_Id1)
                .Index(t => t.Receiver_Id)
                .Index(t => t.Sender_Id)
                .Index(t => t.Sender_Id1);
            
            CreateTable(
                "dbo.KeylolUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IdCode = c.String(nullable: false, maxLength: 5),
                        RegisterTime = c.DateTime(nullable: false),
                        RegisterIp = c.String(nullable: false, maxLength: 64),
                        GamerTag = c.String(nullable: false, maxLength: 100),
                        AvatarImage = c.String(nullable: false, maxLength: 64),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AutoShareOnAddingNewFriend = c.Boolean(nullable: false),
                        AutoShareOnUnlockingAchievement = c.Boolean(nullable: false),
                        AutoShareOnAcquiringNewGame = c.Boolean(nullable: false),
                        AutoShareOnJoiningGroup = c.Boolean(nullable: false),
                        AutoShareOnCreatingGroup = c.Boolean(nullable: false),
                        AutoShareOnUpdatingWishlist = c.Boolean(nullable: false),
                        AutoShareOnPublishingReview = c.Boolean(nullable: false),
                        AutoShareOnUploadingScreenshot = c.Boolean(nullable: false),
                        AutoShareOnAddingVideo = c.Boolean(nullable: false),
                        AutoShareOnAddingFavorite = c.Boolean(nullable: false),
                        EmailNotifyOnArticleReplied = c.Boolean(nullable: false),
                        EmailNotifyOnCommentReplied = c.Boolean(nullable: false),
                        EmailNotifyOnEditorRecommended = c.Boolean(nullable: false),
                        EmailNotifyOnMessageReceived = c.Boolean(nullable: false),
                        EmailNotifyOnAdvertisement = c.Boolean(nullable: false),
                        MessageNotifyOnArticleReplied = c.Boolean(nullable: false),
                        MessageNotifyOnCommentReplied = c.Boolean(nullable: false),
                        MessageNotifyOnEditorRecommended = c.Boolean(nullable: false),
                        MessageNotifyOnArticleLiked = c.Boolean(nullable: false),
                        MessageNotifyOnCommentLiked = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.IdCode, unique: true)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Content = c.String(nullable: false),
                        PublishTime = c.DateTime(nullable: false),
                        Archived = c.Boolean(nullable: false),
                        Article_Id = c.String(nullable: false, maxLength: 128),
                        Commentator_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entries", t => t.Article_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Commentator_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.Commentator_Id);
            
            CreateTable(
                "dbo.Entries",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PublishTime = c.DateTime(nullable: false),
                        Title = c.String(maxLength: 120),
                        Content = c.String(),
                        Vote = c.Int(),
                        Muted = c.Boolean(),
                        Archived = c.Boolean(),
                        GlobalRecommended = c.Boolean(),
                        SequenceNumberForAuthor = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Principal_Id = c.String(nullable: false, maxLength: 128),
                        VoteForPoint_Id = c.String(maxLength: 128),
                        RecommendedArticle_Id = c.String(maxLength: 128),
                        Type_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProfilePoints", t => t.Principal_Id)
                .ForeignKey("dbo.NormalPoints", t => t.VoteForPoint_Id)
                .ForeignKey("dbo.Entries", t => t.RecommendedArticle_Id)
                .ForeignKey("dbo.ArticleTypes", t => t.Type_Id)
                .Index(t => t.SequenceNumberForAuthor)
                .Index(t => t.Principal_Id)
                .Index(t => t.VoteForPoint_Id)
                .Index(t => t.RecommendedArticle_Id)
                .Index(t => t.Type_Id);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Time = c.DateTime(nullable: false),
                        OldTitle = c.String(maxLength: 120),
                        OldContent = c.String(),
                        Ip = c.String(maxLength: 64),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Article_Id = c.String(maxLength: 128),
                        Editor_Id = c.String(maxLength: 128),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entries", t => t.Article_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Editor_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.User_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.Editor_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Likes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Time = c.DateTime(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Article_Id = c.String(maxLength: 128),
                        Operator_Id = c.String(nullable: false, maxLength: 128),
                        Comment_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entries", t => t.Article_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Operator_Id)
                .ForeignKey("dbo.Comments", t => t.Comment_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.Operator_Id)
                .Index(t => t.Comment_Id);
            
            CreateTable(
                "dbo.ArticleTypes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Category = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 20),
                        AllowVote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .ForeignKey("dbo.Roles", t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.PointAssociations",
                c => new
                    {
                        ToPoint_Id = c.String(nullable: false, maxLength: 128),
                        ByPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ToPoint_Id, t.ByPoint_Id })
                .ForeignKey("dbo.NormalPoints", t => t.ToPoint_Id)
                .ForeignKey("dbo.NormalPoints", t => t.ByPoint_Id)
                .Index(t => t.ToPoint_Id)
                .Index(t => t.ByPoint_Id);
            
            CreateTable(
                "dbo.EntryPointPushes",
                c => new
                    {
                        Entry_Id = c.String(nullable: false, maxLength: 128),
                        NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Entry_Id, t.NormalPoint_Id })
                .ForeignKey("dbo.Entries", t => t.Entry_Id)
                .ForeignKey("dbo.NormalPoints", t => t.NormalPoint_Id)
                .Index(t => t.Entry_Id)
                .Index(t => t.NormalPoint_Id);
            
            CreateTable(
                "dbo.PointArticleRecommendation",
                c => new
                    {
                        NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                        Article_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.NormalPoint_Id, t.Article_Id })
                .ForeignKey("dbo.NormalPoints", t => t.NormalPoint_Id)
                .ForeignKey("dbo.Entries", t => t.Article_Id)
                .Index(t => t.NormalPoint_Id)
                .Index(t => t.Article_Id);
            
            CreateTable(
                "dbo.PointStaffs",
                c => new
                    {
                        NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                        KeylolUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.NormalPoint_Id, t.KeylolUser_Id })
                .ForeignKey("dbo.NormalPoints", t => t.NormalPoint_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.KeylolUser_Id)
                .Index(t => t.NormalPoint_Id)
                .Index(t => t.KeylolUser_Id);
            
            CreateTable(
                "dbo.LikeMessagePayload",
                c => new
                    {
                        LikeMessage_Id = c.String(nullable: false, maxLength: 128),
                        Like_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LikeMessage_Id, t.Like_Id })
                .ForeignKey("dbo.Messages", t => t.LikeMessage_Id)
                .ForeignKey("dbo.Likes", t => t.Like_Id)
                .Index(t => t.LikeMessage_Id)
                .Index(t => t.Like_Id);
            
            CreateTable(
                "dbo.CommentReplies",
                c => new
                    {
                        ByComment_Id = c.String(nullable: false, maxLength: 128),
                        ToComment_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ByComment_Id, t.ToComment_Id })
                .ForeignKey("dbo.Comments", t => t.ByComment_Id)
                .ForeignKey("dbo.Comments", t => t.ToComment_Id)
                .Index(t => t.ByComment_Id)
                .Index(t => t.ToComment_Id);
            
            CreateTable(
                "dbo.UserPointSubscriptions",
                c => new
                    {
                        KeylolUser_Id = c.String(nullable: false, maxLength: 128),
                        Point_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.KeylolUser_Id, t.Point_Id })
                .ForeignKey("dbo.KeylolUsers", t => t.KeylolUser_Id)
                .Index(t => t.KeylolUser_Id);
            
            CreateTable(
                "dbo.NormalPoints",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        BackgroundImage = c.String(nullable: false, maxLength: 64),
                        Type = c.Int(nullable: false),
                        IdCode = c.String(nullable: false, maxLength: 5),
                        ChineseName = c.String(nullable: false, maxLength: 150),
                        EnglishName = c.String(nullable: false, maxLength: 150),
                        PreferedName = c.Int(nullable: false),
                        Aliases = c.String(nullable: false, maxLength: 32),
                        StoreLink = c.String(nullable: false, maxLength: 512),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.IdCode, unique: true);
            
            CreateTable(
                "dbo.ProfilePoints",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        BackgroundImage = c.String(nullable: false, maxLength: 64),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProfilePoints", "Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.NormalPoints", "Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.UserPointSubscriptions", "KeylolUser_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Messages", "Sender_Id1", "dbo.KeylolUsers");
            DropForeignKey("dbo.Messages", "Sender_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Messages", "Receiver_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Logs", "User_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.CommentReplies", "ToComment_Id", "dbo.Comments");
            DropForeignKey("dbo.CommentReplies", "ByComment_Id", "dbo.Comments");
            DropForeignKey("dbo.Messages", "Target_Id1", "dbo.Comments");
            DropForeignKey("dbo.Messages", "Comment_Id2", "dbo.Comments");
            DropForeignKey("dbo.Messages", "Comment_Id1", "dbo.Comments");
            DropForeignKey("dbo.Comments", "Commentator_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Comments", "Article_Id", "dbo.Entries");
            DropForeignKey("dbo.Entries", "Type_Id", "dbo.ArticleTypes");
            DropForeignKey("dbo.Messages", "Target_Id", "dbo.Entries");
            DropForeignKey("dbo.Messages", "Article_Id5", "dbo.Entries");
            DropForeignKey("dbo.Messages", "Article_Id4", "dbo.Entries");
            DropForeignKey("dbo.Messages", "Article_Id3", "dbo.Entries");
            DropForeignKey("dbo.Messages", "Article_Id2", "dbo.Entries");
            DropForeignKey("dbo.Messages", "Article_Id1", "dbo.Entries");
            DropForeignKey("dbo.Entries", "RecommendedArticle_Id", "dbo.Entries");
            DropForeignKey("dbo.Messages", "Comment_Id", "dbo.Comments");
            DropForeignKey("dbo.Messages", "Article_Id", "dbo.Entries");
            DropForeignKey("dbo.LikeMessagePayload", "Like_Id", "dbo.Likes");
            DropForeignKey("dbo.LikeMessagePayload", "LikeMessage_Id", "dbo.Messages");
            DropForeignKey("dbo.Likes", "Comment_Id", "dbo.Comments");
            DropForeignKey("dbo.Likes", "Operator_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Likes", "Article_Id", "dbo.Entries");
            DropForeignKey("dbo.Logs", "Editor_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Logs", "Article_Id", "dbo.Entries");
            DropForeignKey("dbo.Entries", "VoteForPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.PointStaffs", "KeylolUser_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.PointStaffs", "NormalPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.Messages", "Point_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.PointArticleRecommendation", "Article_Id", "dbo.Entries");
            DropForeignKey("dbo.PointArticleRecommendation", "NormalPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.Entries", "Principal_Id", "dbo.ProfilePoints");
            DropForeignKey("dbo.EntryPointPushes", "NormalPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.EntryPointPushes", "Entry_Id", "dbo.Entries");
            DropForeignKey("dbo.PointAssociations", "ByPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.PointAssociations", "ToPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.KeylolUsers");
            DropIndex("dbo.ProfilePoints", new[] { "Id" });
            DropIndex("dbo.NormalPoints", new[] { "IdCode" });
            DropIndex("dbo.NormalPoints", new[] { "Id" });
            DropIndex("dbo.UserPointSubscriptions", new[] { "KeylolUser_Id" });
            DropIndex("dbo.CommentReplies", new[] { "ToComment_Id" });
            DropIndex("dbo.CommentReplies", new[] { "ByComment_Id" });
            DropIndex("dbo.LikeMessagePayload", new[] { "Like_Id" });
            DropIndex("dbo.LikeMessagePayload", new[] { "LikeMessage_Id" });
            DropIndex("dbo.PointStaffs", new[] { "KeylolUser_Id" });
            DropIndex("dbo.PointStaffs", new[] { "NormalPoint_Id" });
            DropIndex("dbo.PointArticleRecommendation", new[] { "Article_Id" });
            DropIndex("dbo.PointArticleRecommendation", new[] { "NormalPoint_Id" });
            DropIndex("dbo.EntryPointPushes", new[] { "NormalPoint_Id" });
            DropIndex("dbo.EntryPointPushes", new[] { "Entry_Id" });
            DropIndex("dbo.PointAssociations", new[] { "ByPoint_Id" });
            DropIndex("dbo.PointAssociations", new[] { "ToPoint_Id" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.Likes", new[] { "Comment_Id" });
            DropIndex("dbo.Likes", new[] { "Operator_Id" });
            DropIndex("dbo.Likes", new[] { "Article_Id" });
            DropIndex("dbo.Logs", new[] { "User_Id" });
            DropIndex("dbo.Logs", new[] { "Editor_Id" });
            DropIndex("dbo.Logs", new[] { "Article_Id" });
            DropIndex("dbo.Entries", new[] { "Type_Id" });
            DropIndex("dbo.Entries", new[] { "RecommendedArticle_Id" });
            DropIndex("dbo.Entries", new[] { "VoteForPoint_Id" });
            DropIndex("dbo.Entries", new[] { "Principal_Id" });
            DropIndex("dbo.Entries", new[] { "SequenceNumberForAuthor" });
            DropIndex("dbo.Comments", new[] { "Commentator_Id" });
            DropIndex("dbo.Comments", new[] { "Article_Id" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            DropIndex("dbo.KeylolUsers", "UserNameIndex");
            DropIndex("dbo.KeylolUsers", new[] { "IdCode" });
            DropIndex("dbo.Messages", new[] { "Sender_Id1" });
            DropIndex("dbo.Messages", new[] { "Sender_Id" });
            DropIndex("dbo.Messages", new[] { "Receiver_Id" });
            DropIndex("dbo.Messages", new[] { "Target_Id1" });
            DropIndex("dbo.Messages", new[] { "Comment_Id2" });
            DropIndex("dbo.Messages", new[] { "Comment_Id1" });
            DropIndex("dbo.Messages", new[] { "Target_Id" });
            DropIndex("dbo.Messages", new[] { "Article_Id5" });
            DropIndex("dbo.Messages", new[] { "Article_Id4" });
            DropIndex("dbo.Messages", new[] { "Article_Id3" });
            DropIndex("dbo.Messages", new[] { "Article_Id2" });
            DropIndex("dbo.Messages", new[] { "Article_Id1" });
            DropIndex("dbo.Messages", new[] { "Comment_Id" });
            DropIndex("dbo.Messages", new[] { "Article_Id" });
            DropIndex("dbo.Messages", new[] { "Point_Id" });
            DropTable("dbo.ProfilePoints");
            DropTable("dbo.NormalPoints");
            DropTable("dbo.UserPointSubscriptions");
            DropTable("dbo.CommentReplies");
            DropTable("dbo.LikeMessagePayload");
            DropTable("dbo.PointStaffs");
            DropTable("dbo.PointArticleRecommendation");
            DropTable("dbo.EntryPointPushes");
            DropTable("dbo.PointAssociations");
            DropTable("dbo.Roles");
            DropTable("dbo.UserRoles");
            DropTable("dbo.UserLogins");
            DropTable("dbo.ArticleTypes");
            DropTable("dbo.Likes");
            DropTable("dbo.Logs");
            DropTable("dbo.Entries");
            DropTable("dbo.Comments");
            DropTable("dbo.UserClaims");
            DropTable("dbo.KeylolUsers");
            DropTable("dbo.Messages");
        }
    }
}
