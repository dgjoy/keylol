namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MorePointProperties1 : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Points", "BackgroundImage", "HeaderImage");
            RenameColumn("dbo.Points", "CoverImage", "CapsuleImage");
            DropIndex("dbo.Points", new[] { "SteamAppId" });
            AddColumn("dbo.Points", "InboxHeaderImage", c => c.String(nullable: false, maxLength: 256));
            AddColumn("dbo.Points", "SteamPrice", c => c.Double());
            AddColumn("dbo.Points", "SteamDiscount", c => c.Double());
            AddColumn("dbo.Points", "SonkwoProductId", c => c.Int());
            AddColumn("dbo.Points", "SonkwoPrice", c => c.Double());
            AddColumn("dbo.Points", "SonkwoDiscount", c => c.Double());
            AddColumn("dbo.Points", "UplayLink", c => c.String());
            AddColumn("dbo.Points", "UplayPrice", c => c.Double());
            AddColumn("dbo.Points", "XboxLink", c => c.String());
            AddColumn("dbo.Points", "XboxPrice", c => c.Double());
            AddColumn("dbo.Points", "PlayStationLink", c => c.String());
            AddColumn("dbo.Points", "MediaHeaderImage", c => c.String(nullable: false, maxLength: 256));
            AddColumn("dbo.Points", "Media", c => c.String(nullable: false));
            AddColumn("dbo.Points", "ChineseAvailability", c => c.String(nullable: false));
            AddColumn("dbo.Points", "EmblemImage", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.Points", "SteamAppId", c => c.Int());
            CreateIndex("dbo.Points", "SteamAppId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Points", new[] { "SteamAppId" });
            AlterColumn("dbo.Points", "SteamAppId", c => c.Int(nullable: false));
            DropColumn("dbo.Points", "EmblemImage");
            DropColumn("dbo.Points", "ChineseAvailability");
            DropColumn("dbo.Points", "Media");
            DropColumn("dbo.Points", "MediaHeaderImage");
            DropColumn("dbo.Points", "PlayStationLink");
            DropColumn("dbo.Points", "XboxPrice");
            DropColumn("dbo.Points", "XboxLink");
            DropColumn("dbo.Points", "UplayPrice");
            DropColumn("dbo.Points", "UplayLink");
            DropColumn("dbo.Points", "SonkwoDiscount");
            DropColumn("dbo.Points", "SonkwoPrice");
            DropColumn("dbo.Points", "SonkwoProductId");
            DropColumn("dbo.Points", "SteamDiscount");
            DropColumn("dbo.Points", "SteamPrice");
            DropColumn("dbo.Points", "InboxHeaderImage");
            CreateIndex("dbo.Points", "SteamAppId");
            RenameColumn("dbo.Points", "CapsuleImage", "CoverImage");
            RenameColumn("dbo.Points", "HeaderImage", "BackgroundImage");
        }
    }
}
