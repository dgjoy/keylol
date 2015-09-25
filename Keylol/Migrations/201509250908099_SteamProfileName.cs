namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamProfileName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "SteamProfileName", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.SteamBots", "SteamId", c => c.String(maxLength: 64));
            AlterColumn("dbo.SteamBots", "SessionId", c => c.String(maxLength: 128));
            AlterColumn("dbo.SteamBindingTokens", "SteamId", c => c.String(maxLength: 64));
            AlterColumn("dbo.SteamLoginTokens", "SteamId", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SteamLoginTokens", "SteamId", c => c.String());
            AlterColumn("dbo.SteamBindingTokens", "SteamId", c => c.String());
            AlterColumn("dbo.SteamBots", "SessionId", c => c.String());
            AlterColumn("dbo.SteamBots", "SteamId", c => c.String());
            DropColumn("dbo.KeylolUsers", "SteamProfileName");
        }
    }
}
