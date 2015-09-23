namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserSteamIdIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.KeylolUsers", "SteamId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.KeylolUsers", new[] { "SteamId" });
        }
    }
}
