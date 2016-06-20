namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MessageCountWithMoreIndexes : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Points", new[] { "MultiPlayer" });
            DropIndex("dbo.Points", new[] { "SinglePlayer" });
            DropIndex("dbo.Points", new[] { "Coop" });
            DropIndex("dbo.Points", new[] { "CaptionsAvailable" });
            DropIndex("dbo.Points", new[] { "CommentaryAvailable" });
            DropIndex("dbo.Points", new[] { "IncludeLevelEditor" });
            DropIndex("dbo.Points", new[] { "Achievements" });
            DropIndex("dbo.Points", new[] { "Cloud" });
            DropIndex("dbo.Points", new[] { "LocalCoop" });
            DropIndex("dbo.Points", new[] { "SteamTradingCards" });
            DropIndex("dbo.Points", new[] { "SteamWorkshop" });
            DropIndex("dbo.Points", new[] { "InAppPurchases" });
            DropIndex("dbo.Points", new[] { "ReleaseDate" });
            AddColumn("dbo.Messages", "UserId", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "Count", c => c.Int(nullable: false));
            CreateIndex("dbo.Activities", "Rating");
            CreateIndex("dbo.Points", "CreateTime");
            CreateIndex("dbo.Articles", "Rating");
            CreateIndex("dbo.Feeds", "Time");
            CreateIndex("dbo.Feeds", "EntryType");
            CreateIndex("dbo.Messages", "UserId");
            CreateIndex("dbo.PointRelationships", "Relationship");
            CreateIndex("dbo.UserSteamGameRecords", "TotalPlayedTime");
            AddForeignKey("dbo.Messages", "UserId", "dbo.KeylolUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Messages", "UserId", "dbo.KeylolUsers");
            DropIndex("dbo.UserSteamGameRecords", new[] { "TotalPlayedTime" });
            DropIndex("dbo.PointRelationships", new[] { "Relationship" });
            DropIndex("dbo.Messages", new[] { "UserId" });
            DropIndex("dbo.Feeds", new[] { "EntryType" });
            DropIndex("dbo.Feeds", new[] { "Time" });
            DropIndex("dbo.Articles", new[] { "Rating" });
            DropIndex("dbo.Points", new[] { "CreateTime" });
            DropIndex("dbo.Activities", new[] { "Rating" });
            DropColumn("dbo.Messages", "Count");
            DropColumn("dbo.Messages", "UserId");
            CreateIndex("dbo.Points", "ReleaseDate");
            CreateIndex("dbo.Points", "InAppPurchases");
            CreateIndex("dbo.Points", "SteamWorkshop");
            CreateIndex("dbo.Points", "SteamTradingCards");
            CreateIndex("dbo.Points", "LocalCoop");
            CreateIndex("dbo.Points", "Cloud");
            CreateIndex("dbo.Points", "Achievements");
            CreateIndex("dbo.Points", "IncludeLevelEditor");
            CreateIndex("dbo.Points", "CommentaryAvailable");
            CreateIndex("dbo.Points", "CaptionsAvailable");
            CreateIndex("dbo.Points", "Coop");
            CreateIndex("dbo.Points", "SinglePlayer");
            CreateIndex("dbo.Points", "MultiPlayer");
        }
    }
}
