namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BotCookies : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SteamBots", "Cookies", c => c.String());
            AddColumn("dbo.SteamBots", "Enabled", c => c.Boolean(nullable: false, defaultValue: true));
            CreateIndex("dbo.SteamBots", "Enabled");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SteamBots", new[] { "Enabled" });
            DropColumn("dbo.SteamBots", "Enabled");
            DropColumn("dbo.SteamBots", "Cookies");
        }
    }
}
