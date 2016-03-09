namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamNotifyOptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnArticleReplied", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnCommentReplied", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnArticleLiked", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnCommentLiked", c => c.Boolean(nullable: false, defaultValue: true));
            DropColumn("dbo.KeylolUsers", "AutoShareOnAddingNewFriend");
            DropColumn("dbo.KeylolUsers", "AutoShareOnUnlockingAchievement");
            DropColumn("dbo.KeylolUsers", "AutoShareOnAcquiringNewGame");
            DropColumn("dbo.KeylolUsers", "AutoShareOnJoiningGroup");
            DropColumn("dbo.KeylolUsers", "AutoShareOnCreatingGroup");
            DropColumn("dbo.KeylolUsers", "AutoShareOnUpdatingWishlist");
            DropColumn("dbo.KeylolUsers", "AutoShareOnPublishingReview");
            DropColumn("dbo.KeylolUsers", "AutoShareOnUploadingScreenshot");
            DropColumn("dbo.KeylolUsers", "AutoShareOnAddingVideo");
            DropColumn("dbo.KeylolUsers", "AutoShareOnAddingFavorite");
            DropColumn("dbo.KeylolUsers", "EmailNotifyOnArticleReplied");
            DropColumn("dbo.KeylolUsers", "EmailNotifyOnCommentReplied");
            DropColumn("dbo.KeylolUsers", "EmailNotifyOnEditorRecommended");
            DropColumn("dbo.KeylolUsers", "EmailNotifyOnMessageReceived");
            DropColumn("dbo.KeylolUsers", "EmailNotifyOnAdvertisement");
            DropColumn("dbo.KeylolUsers", "MessageNotifyOnArticleReplied");
            DropColumn("dbo.KeylolUsers", "MessageNotifyOnCommentReplied");
            DropColumn("dbo.KeylolUsers", "MessageNotifyOnEditorRecommended");
            DropColumn("dbo.KeylolUsers", "MessageNotifyOnArticleLiked");
            DropColumn("dbo.KeylolUsers", "MessageNotifyOnCommentLiked");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KeylolUsers", "MessageNotifyOnCommentLiked", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "MessageNotifyOnArticleLiked", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "MessageNotifyOnEditorRecommended", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "MessageNotifyOnCommentReplied", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "MessageNotifyOnArticleReplied", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "EmailNotifyOnAdvertisement", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "EmailNotifyOnMessageReceived", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "EmailNotifyOnEditorRecommended", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "EmailNotifyOnCommentReplied", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "EmailNotifyOnArticleReplied", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnAddingFavorite", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnAddingVideo", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnUploadingScreenshot", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnPublishingReview", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnUpdatingWishlist", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnCreatingGroup", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnJoiningGroup", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnAcquiringNewGame", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnUnlockingAchievement", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoShareOnAddingNewFriend", c => c.Boolean(nullable: false));
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnCommentLiked");
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnArticleLiked");
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnCommentReplied");
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnArticleReplied");
        }
    }
}
