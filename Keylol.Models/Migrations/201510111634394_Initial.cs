namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Likes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Time = c.DateTime(nullable: false),
                        ReadByTargetUser = c.Boolean(nullable: false),
                        OperatorId = c.String(nullable: false, maxLength: 128),
                        ArticleId = c.String(maxLength: 128),
                        CommentId = c.String(maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comments", t => t.CommentId)
                .ForeignKey("dbo.KeylolUsers", t => t.OperatorId)
                .ForeignKey("dbo.Entries", t => t.ArticleId)
                .Index(t => t.OperatorId)
                .Index(t => t.ArticleId)
                .Index(t => t.CommentId);
            
            CreateTable(
                "dbo.Entries",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PublishTime = c.DateTime(nullable: false),
                        PrincipalId = c.String(nullable: false, maxLength: 128),
                        TypeId = c.String(maxLength: 128),
                        Title = c.String(maxLength: 120),
                        Content = c.String(),
                        Vote = c.Int(),
                        SequenceNumberForAuthor = c.Int(),
                        VoteForPointId = c.String(maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProfilePoints", t => t.PrincipalId)
                .ForeignKey("dbo.NormalPoints", t => t.VoteForPointId)
                .ForeignKey("dbo.ArticleTypes", t => t.TypeId)
                .Index(t => t.PrincipalId)
                .Index(t => t.TypeId)
                .Index(t => t.SequenceNumberForAuthor)
                .Index(t => t.VoteForPointId);
            
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
                        SteamId = c.String(maxLength: 64),
                        SteamProfileName = c.String(nullable: false, maxLength: 64),
                        SteamBindingTime = c.DateTime(nullable: false),
                        SteamBindingLockEnabled = c.Boolean(nullable: false),
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
                        SteamBotId = c.String(maxLength: 128),
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
                .ForeignKey("dbo.SteamBots", t => t.SteamBotId)
                .Index(t => t.IdCode, unique: true)
                .Index(t => t.SteamId)
                .Index(t => t.SteamBotId)
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
                        ReadByArticleAuthor = c.Boolean(nullable: false),
                        CommentatorId = c.String(nullable: false, maxLength: 128),
                        ArticleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entries", t => t.ArticleId)
                .ForeignKey("dbo.KeylolUsers", t => t.CommentatorId)
                .Index(t => t.CommentatorId)
                .Index(t => t.ArticleId);
            
            CreateTable(
                "dbo.CommentReplies",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CommentId = c.String(nullable: false, maxLength: 128),
                        ReadByCommentAuthor = c.Boolean(nullable: false),
                        ReplyId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comments", t => t.CommentId)
                .ForeignKey("dbo.Comments", t => t.ReplyId)
                .Index(t => t.CommentId)
                .Index(t => t.ReplyId);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Time = c.DateTime(nullable: false),
                        ArticleId = c.String(maxLength: 128),
                        EditorId = c.String(maxLength: 128),
                        OldTitle = c.String(maxLength: 120),
                        OldContent = c.String(),
                        Ip = c.String(maxLength: 64),
                        UserId = c.String(maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entries", t => t.ArticleId)
                .ForeignKey("dbo.KeylolUsers", t => t.EditorId)
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.ArticleId)
                .Index(t => t.EditorId)
                .Index(t => t.UserId);
            
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
                "dbo.SteamBots",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        SteamUserName = c.String(nullable: false, maxLength: 64),
                        SteamPassword = c.String(nullable: false, maxLength: 64),
                        SteamId = c.String(maxLength: 64),
                        FriendCount = c.Int(nullable: false),
                        FriendUpperLimit = c.Int(nullable: false),
                        Online = c.Boolean(nullable: false),
                        SessionId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.SteamUserName, unique: true);
            
            CreateTable(
                "dbo.SteamBindingTokens",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 8),
                        BrowserConnectionId = c.String(nullable: false, maxLength: 128),
                        SteamId = c.String(maxLength: 64),
                        BotId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SteamBots", t => t.BotId)
                .Index(t => t.Code, unique: true)
                .Index(t => t.BrowserConnectionId)
                .Index(t => t.BotId);
            
            CreateTable(
                "dbo.ArticleTypes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 20),
                        Description = c.String(nullable: false, maxLength: 32),
                        AllowVote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.SteamLoginTokens",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 4),
                        BrowserConnectionId = c.String(nullable: false, maxLength: 128),
                        SteamId = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true)
                .Index(t => t.BrowserConnectionId);
            
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
                "dbo.ArticlePointPushes",
                c => new
                    {
                        Article_Id = c.String(nullable: false, maxLength: 128),
                        NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Article_Id, t.NormalPoint_Id })
                .ForeignKey("dbo.Entries", t => t.Article_Id)
                .ForeignKey("dbo.NormalPoints", t => t.NormalPoint_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.NormalPoint_Id);
            
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
                        Aliases = c.String(nullable: false, maxLength: 32),
                        StoreLink = c.String(nullable: false, maxLength: 512),
                    })
                .PrimaryKey(t => t.Id)
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
            DropForeignKey("dbo.Entries", "TypeId", "dbo.ArticleTypes");
            DropForeignKey("dbo.Likes", "ArticleId", "dbo.Entries");
            DropForeignKey("dbo.ArticlePointPushes", "NormalPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.ArticlePointPushes", "Article_Id", "dbo.Entries");
            DropForeignKey("dbo.Entries", "VoteForPointId", "dbo.NormalPoints");
            DropForeignKey("dbo.PointStaffs", "KeylolUser_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.PointStaffs", "NormalPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.UserPointSubscriptions", "KeylolUser_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.KeylolUsers", "SteamBotId", "dbo.SteamBots");
            DropForeignKey("dbo.SteamBindingTokens", "BotId", "dbo.SteamBots");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Entries", "PrincipalId", "dbo.ProfilePoints");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Logs", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Likes", "OperatorId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Logs", "EditorId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Logs", "ArticleId", "dbo.Entries");
            DropForeignKey("dbo.Likes", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.CommentReplies", "ReplyId", "dbo.Comments");
            DropForeignKey("dbo.CommentReplies", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.Comments", "CommentatorId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Comments", "ArticleId", "dbo.Entries");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.PointAssociations", "ByPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.PointAssociations", "ToPoint_Id", "dbo.NormalPoints");
            DropIndex("dbo.ProfilePoints", new[] { "Id" });
            DropIndex("dbo.NormalPoints", new[] { "IdCode" });
            DropIndex("dbo.NormalPoints", new[] { "Id" });
            DropIndex("dbo.ArticlePointPushes", new[] { "NormalPoint_Id" });
            DropIndex("dbo.ArticlePointPushes", new[] { "Article_Id" });
            DropIndex("dbo.PointStaffs", new[] { "KeylolUser_Id" });
            DropIndex("dbo.PointStaffs", new[] { "NormalPoint_Id" });
            DropIndex("dbo.UserPointSubscriptions", new[] { "KeylolUser_Id" });
            DropIndex("dbo.PointAssociations", new[] { "ByPoint_Id" });
            DropIndex("dbo.PointAssociations", new[] { "ToPoint_Id" });
            DropIndex("dbo.SteamLoginTokens", new[] { "BrowserConnectionId" });
            DropIndex("dbo.SteamLoginTokens", new[] { "Code" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.SteamBindingTokens", new[] { "BotId" });
            DropIndex("dbo.SteamBindingTokens", new[] { "BrowserConnectionId" });
            DropIndex("dbo.SteamBindingTokens", new[] { "Code" });
            DropIndex("dbo.SteamBots", new[] { "SteamUserName" });
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.Logs", new[] { "UserId" });
            DropIndex("dbo.Logs", new[] { "EditorId" });
            DropIndex("dbo.Logs", new[] { "ArticleId" });
            DropIndex("dbo.CommentReplies", new[] { "ReplyId" });
            DropIndex("dbo.CommentReplies", new[] { "CommentId" });
            DropIndex("dbo.Comments", new[] { "ArticleId" });
            DropIndex("dbo.Comments", new[] { "CommentatorId" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            DropIndex("dbo.KeylolUsers", "UserNameIndex");
            DropIndex("dbo.KeylolUsers", new[] { "SteamBotId" });
            DropIndex("dbo.KeylolUsers", new[] { "SteamId" });
            DropIndex("dbo.KeylolUsers", new[] { "IdCode" });
            DropIndex("dbo.Entries", new[] { "VoteForPointId" });
            DropIndex("dbo.Entries", new[] { "SequenceNumberForAuthor" });
            DropIndex("dbo.Entries", new[] { "TypeId" });
            DropIndex("dbo.Entries", new[] { "PrincipalId" });
            DropIndex("dbo.Likes", new[] { "CommentId" });
            DropIndex("dbo.Likes", new[] { "ArticleId" });
            DropIndex("dbo.Likes", new[] { "OperatorId" });
            DropTable("dbo.ProfilePoints");
            DropTable("dbo.NormalPoints");
            DropTable("dbo.ArticlePointPushes");
            DropTable("dbo.PointStaffs");
            DropTable("dbo.UserPointSubscriptions");
            DropTable("dbo.PointAssociations");
            DropTable("dbo.SteamLoginTokens");
            DropTable("dbo.Roles");
            DropTable("dbo.ArticleTypes");
            DropTable("dbo.SteamBindingTokens");
            DropTable("dbo.SteamBots");
            DropTable("dbo.UserRoles");
            DropTable("dbo.UserLogins");
            DropTable("dbo.Logs");
            DropTable("dbo.CommentReplies");
            DropTable("dbo.Comments");
            DropTable("dbo.UserClaims");
            DropTable("dbo.KeylolUsers");
            DropTable("dbo.Entries");
            DropTable("dbo.Likes");
        }
    }
}
