namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserPreferences : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "OpenInNewWindow", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.KeylolUsers", "NotifyOnArtivleReplied", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "NotifyOnCommentReplied", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "NotifyOnArtivleLiked", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "NotifyOnCommentLiked", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnSpotlighted", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnMissive", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnMissive");
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnSpotlighted");
            DropColumn("dbo.KeylolUsers", "NotifyOnCommentLiked");
            DropColumn("dbo.KeylolUsers", "NotifyOnArtivleLiked");
            DropColumn("dbo.KeylolUsers", "NotifyOnCommentReplied");
            DropColumn("dbo.KeylolUsers", "NotifyOnArtivleReplied");
            DropColumn("dbo.KeylolUsers", "OpenInNewWindow");
        }
    }
}
