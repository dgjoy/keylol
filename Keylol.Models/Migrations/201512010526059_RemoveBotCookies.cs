namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveBotCookies : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SteamBots", "Cookies");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SteamBots", "Cookies", c => c.String());
        }
    }
}
