namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoreIndexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Likes", "Time");
            CreateIndex("dbo.Likes", "Backout");
            CreateIndex("dbo.Entries", "PublishTime");
            CreateIndex("dbo.Entries", "SequenceNumber", unique: true);
            CreateIndex("dbo.Comments", "PublishTime");
            CreateIndex("dbo.Logs", "Time");
            CreateIndex("dbo.Logs", "Ip");
            CreateIndex("dbo.SteamBots", "SteamId");
            CreateIndex("dbo.SteamBots", "FriendCount");
            CreateIndex("dbo.SteamBots", "Online");
            CreateIndex("dbo.SteamBots", "SessionId");
            CreateIndex("dbo.SteamBindingTokens", "SteamId");
            CreateIndex("dbo.SteamLoginTokens", "SteamId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SteamLoginTokens", new[] { "SteamId" });
            DropIndex("dbo.SteamBindingTokens", new[] { "SteamId" });
            DropIndex("dbo.SteamBots", new[] { "SessionId" });
            DropIndex("dbo.SteamBots", new[] { "Online" });
            DropIndex("dbo.SteamBots", new[] { "FriendCount" });
            DropIndex("dbo.SteamBots", new[] { "SteamId" });
            DropIndex("dbo.Logs", new[] { "Ip" });
            DropIndex("dbo.Logs", new[] { "Time" });
            DropIndex("dbo.Comments", new[] { "PublishTime" });
            DropIndex("dbo.Entries", new[] { "SequenceNumber" });
            DropIndex("dbo.Entries", new[] { "PublishTime" });
            DropIndex("dbo.Likes", new[] { "Backout" });
            DropIndex("dbo.Likes", new[] { "Time" });
        }
    }
}
