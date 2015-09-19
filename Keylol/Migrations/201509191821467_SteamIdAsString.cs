namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamIdAsString : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.KeylolUsers", new[] { "SteamId" });
            AlterColumn("dbo.KeylolUsers", "SteamId", c => c.String(maxLength: 64));
            AlterColumn("dbo.SteamBots", "SteamId", c => c.String());
            AlterColumn("dbo.SteamBindingTokens", "SteamId", c => c.String());
            AlterColumn("dbo.SteamLoginTokens", "SteamId", c => c.String());
            CreateIndex("dbo.KeylolUsers", "SteamId", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.KeylolUsers", new[] { "SteamId" });
            AlterColumn("dbo.SteamLoginTokens", "SteamId", c => c.Long());
            AlterColumn("dbo.SteamBindingTokens", "SteamId", c => c.Long());
            AlterColumn("dbo.SteamBots", "SteamId", c => c.Long());
            AlterColumn("dbo.KeylolUsers", "SteamId", c => c.Long());
            CreateIndex("dbo.KeylolUsers", "SteamId");
        }
    }
}
