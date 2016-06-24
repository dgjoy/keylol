namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointAttributes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Points", "PlayStationPrice", c => c.Double());
            AddColumn("dbo.Points", "MultiPlayer", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "SinglePlayer", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "Coop", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "CaptionsAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "CommentaryAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "IncludeLevelEditor", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "Achievements", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "Cloud", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "LocalCoop", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "SteamTradingCards", c => c.Boolean(nullable: false));
            AddColumn("dbo.Points", "SteamWorkshop", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Points", "MultiPlayer");
            CreateIndex("dbo.Points", "SinglePlayer");
            CreateIndex("dbo.Points", "Coop");
            CreateIndex("dbo.Points", "CaptionsAvailable");
            CreateIndex("dbo.Points", "CommentaryAvailable");
            CreateIndex("dbo.Points", "IncludeLevelEditor");
            CreateIndex("dbo.Points", "Achievements");
            CreateIndex("dbo.Points", "Cloud");
            CreateIndex("dbo.Points", "LocalCoop");
            CreateIndex("dbo.Points", "SteamTradingCards");
            CreateIndex("dbo.Points", "SteamWorkshop");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Points", new[] { "SteamWorkshop" });
            DropIndex("dbo.Points", new[] { "SteamTradingCards" });
            DropIndex("dbo.Points", new[] { "LocalCoop" });
            DropIndex("dbo.Points", new[] { "Cloud" });
            DropIndex("dbo.Points", new[] { "Achievements" });
            DropIndex("dbo.Points", new[] { "IncludeLevelEditor" });
            DropIndex("dbo.Points", new[] { "CommentaryAvailable" });
            DropIndex("dbo.Points", new[] { "CaptionsAvailable" });
            DropIndex("dbo.Points", new[] { "Coop" });
            DropIndex("dbo.Points", new[] { "SinglePlayer" });
            DropIndex("dbo.Points", new[] { "MultiPlayer" });
            DropColumn("dbo.Points", "SteamWorkshop");
            DropColumn("dbo.Points", "SteamTradingCards");
            DropColumn("dbo.Points", "LocalCoop");
            DropColumn("dbo.Points", "Cloud");
            DropColumn("dbo.Points", "Achievements");
            DropColumn("dbo.Points", "IncludeLevelEditor");
            DropColumn("dbo.Points", "CommentaryAvailable");
            DropColumn("dbo.Points", "CaptionsAvailable");
            DropColumn("dbo.Points", "Coop");
            DropColumn("dbo.Points", "SinglePlayer");
            DropColumn("dbo.Points", "MultiPlayer");
            DropColumn("dbo.Points", "PlayStationPrice");
        }
    }
}
