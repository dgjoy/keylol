namespace Keylol.Migrations
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
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Operator_Id = c.String(nullable: false, maxLength: 128),
                        Comment_Id = c.String(maxLength: 128),
                        Article_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Operator_Id)
                .ForeignKey("dbo.Comments", t => t.Comment_Id)
                .ForeignKey("dbo.Articles", t => t.Article_Id)
                .Index(t => t.Operator_Id)
                .Index(t => t.Comment_Id)
                .Index(t => t.Article_Id);
            
            CreateTable(
                "dbo.Articles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Title = c.String(nullable: false, maxLength: 120),
                        Content = c.String(nullable: false),
                        PublishTime = c.DateTime(nullable: false),
                        LastEditTime = c.DateTime(nullable: false),
                        RecommendedArticle_Id = c.String(maxLength: 128),
                        Type_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Articles", t => t.RecommendedArticle_Id)
                .ForeignKey("dbo.ArticleTypes", t => t.Type_Id)
                .Index(t => t.RecommendedArticle_Id)
                .Index(t => t.Type_Id);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Content = c.String(nullable: false),
                        PublishTime = c.DateTime(nullable: false),
                        LastEditTime = c.DateTime(nullable: false),
                        Article_Id = c.String(nullable: false, maxLength: 128),
                        Commentator_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Articles", t => t.Article_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Commentator_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.Commentator_Id);
            
            CreateTable(
                "dbo.KeylolUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        RegisterTime = c.DateTime(nullable: false),
                        RegisterIp = c.String(),
                        LastVisitTime = c.DateTime(nullable: false),
                        LastVisitIp = c.String(),
                        GamerTag = c.String(maxLength: 100),
                        AutoShareOnAcquiringNewGame = c.Boolean(nullable: false),
                        AutoShareOnPublishingReview = c.Boolean(nullable: false),
                        AutoShareOnUnlockingAchievement = c.Boolean(nullable: false),
                        AutoShareOnUploadingScreenshot = c.Boolean(nullable: false),
                        AutoShareOnAddingFavorite = c.Boolean(nullable: false),
                        EmailNotifyOnArticleReplied = c.Boolean(nullable: false),
                        EmailNotifyOnCommentReplied = c.Boolean(nullable: false),
                        EmailNotifyOnEditorRecommended = c.Boolean(nullable: false),
                        EmailNotifyOnUserMessageReceived = c.Boolean(nullable: false),
                        PreferedLanguageConversionMode = c.Int(nullable: false),
                        ColorVisionDeficiency = c.Boolean(nullable: false),
                        VisionImpairment = c.Boolean(nullable: false),
                        HearingImpairment = c.Boolean(nullable: false),
                        PhotosensitiveEpilepsy = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Points", t => t.Id)
                .Index(t => t.Id)
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
                "dbo.Points",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(maxLength: 150),
                        AlternativeName = c.String(maxLength: 150),
                        UrlFriendlyName = c.String(maxLength: 100),
                        Type = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Content = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Receiver_Id = c.String(nullable: false, maxLength: 128),
                        Like_Id = c.String(maxLength: 128),
                        Comment_Id = c.String(maxLength: 128),
                        Warning_Id = c.String(maxLength: 128),
                        Sender_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Receiver_Id)
                .ForeignKey("dbo.Likes", t => t.Like_Id)
                .ForeignKey("dbo.Comments", t => t.Comment_Id)
                .ForeignKey("dbo.Warnings", t => t.Warning_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Sender_Id)
                .Index(t => t.Receiver_Id)
                .Index(t => t.Like_Id)
                .Index(t => t.Comment_Id)
                .Index(t => t.Warning_Id)
                .Index(t => t.Sender_Id);
            
            CreateTable(
                "dbo.Warnings",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Target_Id = c.String(nullable: false, maxLength: 128),
                        Operator_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Target_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Operator_Id)
                .Index(t => t.Target_Id)
                .Index(t => t.Operator_Id);
            
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
                "dbo.ArticleTypes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Category = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 20),
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
                "dbo.PointAssociations",
                c => new
                    {
                        NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                        Point_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.NormalPoint_Id, t.Point_Id })
                .ForeignKey("dbo.Points", t => t.NormalPoint_Id)
                .ForeignKey("dbo.Points", t => t.Point_Id)
                .Index(t => t.NormalPoint_Id)
                .Index(t => t.Point_Id);
            
            CreateTable(
                "dbo.UserPointSubscriptions",
                c => new
                    {
                        KeylolUser_Id = c.String(nullable: false, maxLength: 128),
                        Point_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.KeylolUser_Id, t.Point_Id })
                .ForeignKey("dbo.KeylolUsers", t => t.KeylolUser_Id)
                .ForeignKey("dbo.Points", t => t.Point_Id)
                .Index(t => t.KeylolUser_Id)
                .Index(t => t.Point_Id);
            
            CreateTable(
                "dbo.CommentReplies",
                c => new
                    {
                        ByComment = c.String(nullable: false, maxLength: 128),
                        ToComment = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ByComment, t.ToComment })
                .ForeignKey("dbo.Comments", t => t.ByComment)
                .ForeignKey("dbo.Comments", t => t.ToComment)
                .Index(t => t.ByComment)
                .Index(t => t.ToComment);
            
            CreateTable(
                "dbo.ArticlePointPushes",
                c => new
                    {
                        Article_Id = c.String(nullable: false, maxLength: 128),
                        Point_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Article_Id, t.Point_Id })
                .ForeignKey("dbo.Articles", t => t.Article_Id)
                .ForeignKey("dbo.Points", t => t.Point_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.Point_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Likes", "Article_Id", "dbo.Articles");
            DropForeignKey("dbo.Articles", "Type_Id", "dbo.ArticleTypes");
            DropForeignKey("dbo.Articles", "RecommendedArticle_Id", "dbo.Articles");
            DropForeignKey("dbo.ArticlePointPushes", "Point_Id", "dbo.Points");
            DropForeignKey("dbo.ArticlePointPushes", "Article_Id", "dbo.Articles");
            DropForeignKey("dbo.CommentReplies", "ToComment", "dbo.Comments");
            DropForeignKey("dbo.CommentReplies", "ByComment", "dbo.Comments");
            DropForeignKey("dbo.Comments", "Commentator_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.UserPointSubscriptions", "Point_Id", "dbo.Points");
            DropForeignKey("dbo.UserPointSubscriptions", "KeylolUser_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Warnings", "Operator_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Warnings", "Target_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Messages", "Sender_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Messages", "Warning_Id", "dbo.Warnings");
            DropForeignKey("dbo.Messages", "Comment_Id", "dbo.Comments");
            DropForeignKey("dbo.Messages", "Like_Id", "dbo.Likes");
            DropForeignKey("dbo.Messages", "Receiver_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.KeylolUsers", "Id", "dbo.Points");
            DropForeignKey("dbo.PointAssociations", "Point_Id", "dbo.Points");
            DropForeignKey("dbo.PointAssociations", "NormalPoint_Id", "dbo.Points");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Likes", "Comment_Id", "dbo.Comments");
            DropForeignKey("dbo.Likes", "Operator_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Comments", "Article_Id", "dbo.Articles");
            DropIndex("dbo.ArticlePointPushes", new[] { "Point_Id" });
            DropIndex("dbo.ArticlePointPushes", new[] { "Article_Id" });
            DropIndex("dbo.CommentReplies", new[] { "ToComment" });
            DropIndex("dbo.CommentReplies", new[] { "ByComment" });
            DropIndex("dbo.UserPointSubscriptions", new[] { "Point_Id" });
            DropIndex("dbo.UserPointSubscriptions", new[] { "KeylolUser_Id" });
            DropIndex("dbo.PointAssociations", new[] { "Point_Id" });
            DropIndex("dbo.PointAssociations", new[] { "NormalPoint_Id" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.Warnings", new[] { "Operator_Id" });
            DropIndex("dbo.Warnings", new[] { "Target_Id" });
            DropIndex("dbo.Messages", new[] { "Sender_Id" });
            DropIndex("dbo.Messages", new[] { "Warning_Id" });
            DropIndex("dbo.Messages", new[] { "Comment_Id" });
            DropIndex("dbo.Messages", new[] { "Like_Id" });
            DropIndex("dbo.Messages", new[] { "Receiver_Id" });
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            DropIndex("dbo.KeylolUsers", "UserNameIndex");
            DropIndex("dbo.KeylolUsers", new[] { "Id" });
            DropIndex("dbo.Comments", new[] { "Commentator_Id" });
            DropIndex("dbo.Comments", new[] { "Article_Id" });
            DropIndex("dbo.Articles", new[] { "Type_Id" });
            DropIndex("dbo.Articles", new[] { "RecommendedArticle_Id" });
            DropIndex("dbo.Likes", new[] { "Article_Id" });
            DropIndex("dbo.Likes", new[] { "Comment_Id" });
            DropIndex("dbo.Likes", new[] { "Operator_Id" });
            DropTable("dbo.ArticlePointPushes");
            DropTable("dbo.CommentReplies");
            DropTable("dbo.UserPointSubscriptions");
            DropTable("dbo.PointAssociations");
            DropTable("dbo.Roles");
            DropTable("dbo.ArticleTypes");
            DropTable("dbo.UserRoles");
            DropTable("dbo.Warnings");
            DropTable("dbo.Messages");
            DropTable("dbo.Points");
            DropTable("dbo.UserLogins");
            DropTable("dbo.UserClaims");
            DropTable("dbo.KeylolUsers");
            DropTable("dbo.Comments");
            DropTable("dbo.Articles");
            DropTable("dbo.Likes");
        }
    }
}
