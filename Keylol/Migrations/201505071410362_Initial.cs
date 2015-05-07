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
                        Comment_Id = c.String(maxLength: 128),
                        Operator_Id = c.String(nullable: false, maxLength: 128),
                        Article_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comments", t => t.Comment_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Operator_Id)
                .ForeignKey("dbo.Pieces", t => t.Article_Id)
                .Index(t => t.Comment_Id)
                .Index(t => t.Operator_Id)
                .Index(t => t.Article_Id);
            
            CreateTable(
                "dbo.Pieces",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PublishTime = c.DateTime(nullable: false),
                        Title = c.String(maxLength: 120),
                        Content = c.String(),
                        LastEditTime = c.DateTime(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Principal_Id = c.String(nullable: false, maxLength: 128),
                        RecommendedArticle_Id = c.String(maxLength: 128),
                        Type_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Points", t => t.Principal_Id)
                .ForeignKey("dbo.Pieces", t => t.RecommendedArticle_Id)
                .ForeignKey("dbo.ArticleTypes", t => t.Type_Id)
                .Index(t => t.Principal_Id)
                .Index(t => t.RecommendedArticle_Id)
                .Index(t => t.Type_Id);
            
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
                .ForeignKey("dbo.Pieces", t => t.Article_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.Commentator_Id)
                .Index(t => t.Article_Id)
                .Index(t => t.Commentator_Id);
            
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
                "dbo.PiecePointPushes",
                c => new
                    {
                        Piece_Id = c.String(nullable: false, maxLength: 128),
                        Point_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Piece_Id, t.Point_Id })
                .ForeignKey("dbo.Pieces", t => t.Piece_Id)
                .ForeignKey("dbo.Points", t => t.Point_Id)
                .Index(t => t.Piece_Id)
                .Index(t => t.Point_Id);
            
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
                .ForeignKey("dbo.Points", t => t.Point_Id)
                .Index(t => t.KeylolUser_Id)
                .Index(t => t.Point_Id);
            
            CreateTable(
                "dbo.PointModerators",
                c => new
                    {
                        NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                        KeylolUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.NormalPoint_Id, t.KeylolUser_Id })
                .ForeignKey("dbo.Points", t => t.NormalPoint_Id)
                .ForeignKey("dbo.KeylolUsers", t => t.KeylolUser_Id)
                .Index(t => t.NormalPoint_Id)
                .Index(t => t.KeylolUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Likes", "Article_Id", "dbo.Pieces");
            DropForeignKey("dbo.Pieces", "Type_Id", "dbo.ArticleTypes");
            DropForeignKey("dbo.Pieces", "RecommendedArticle_Id", "dbo.Pieces");
            DropForeignKey("dbo.PointModerators", "KeylolUser_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.PointModerators", "NormalPoint_Id", "dbo.Points");
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
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Likes", "Operator_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.CommentReplies", "ToComment_Id", "dbo.Comments");
            DropForeignKey("dbo.CommentReplies", "ByComment_Id", "dbo.Comments");
            DropForeignKey("dbo.Likes", "Comment_Id", "dbo.Comments");
            DropForeignKey("dbo.Comments", "Commentator_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Comments", "Article_Id", "dbo.Pieces");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Pieces", "Principal_Id", "dbo.Points");
            DropForeignKey("dbo.PiecePointPushes", "Point_Id", "dbo.Points");
            DropForeignKey("dbo.PiecePointPushes", "Piece_Id", "dbo.Pieces");
            DropForeignKey("dbo.PointAssociations", "Point_Id", "dbo.Points");
            DropForeignKey("dbo.PointAssociations", "NormalPoint_Id", "dbo.Points");
            DropIndex("dbo.PointModerators", new[] { "KeylolUser_Id" });
            DropIndex("dbo.PointModerators", new[] { "NormalPoint_Id" });
            DropIndex("dbo.UserPointSubscriptions", new[] { "Point_Id" });
            DropIndex("dbo.UserPointSubscriptions", new[] { "KeylolUser_Id" });
            DropIndex("dbo.CommentReplies", new[] { "ToComment_Id" });
            DropIndex("dbo.CommentReplies", new[] { "ByComment_Id" });
            DropIndex("dbo.PiecePointPushes", new[] { "Point_Id" });
            DropIndex("dbo.PiecePointPushes", new[] { "Piece_Id" });
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
            DropIndex("dbo.Comments", new[] { "Commentator_Id" });
            DropIndex("dbo.Comments", new[] { "Article_Id" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            DropIndex("dbo.KeylolUsers", "UserNameIndex");
            DropIndex("dbo.KeylolUsers", new[] { "Id" });
            DropIndex("dbo.Pieces", new[] { "Type_Id" });
            DropIndex("dbo.Pieces", new[] { "RecommendedArticle_Id" });
            DropIndex("dbo.Pieces", new[] { "Principal_Id" });
            DropIndex("dbo.Likes", new[] { "Article_Id" });
            DropIndex("dbo.Likes", new[] { "Operator_Id" });
            DropIndex("dbo.Likes", new[] { "Comment_Id" });
            DropTable("dbo.PointModerators");
            DropTable("dbo.UserPointSubscriptions");
            DropTable("dbo.CommentReplies");
            DropTable("dbo.PiecePointPushes");
            DropTable("dbo.PointAssociations");
            DropTable("dbo.Roles");
            DropTable("dbo.ArticleTypes");
            DropTable("dbo.UserRoles");
            DropTable("dbo.Warnings");
            DropTable("dbo.Messages");
            DropTable("dbo.UserLogins");
            DropTable("dbo.Comments");
            DropTable("dbo.UserClaims");
            DropTable("dbo.KeylolUsers");
            DropTable("dbo.Points");
            DropTable("dbo.Pieces");
            DropTable("dbo.Likes");
        }
    }
}
