namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamAppIdResetNull : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE Points SET SteamAppId = NULL WHERE SteamAppId = 0");
        }
        
        public override void Down()
        {
            Sql(@"UPDATE Points SET SteamAppId = 0 WHERE SteamAppId IS NULL");
        }
    }
}
