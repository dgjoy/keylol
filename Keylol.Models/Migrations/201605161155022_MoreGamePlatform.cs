namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoreGamePlatform : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Points", "OriginLink", c => c.String());
            AddColumn("dbo.Points", "OriginPrice", c => c.Double());
            AddColumn("dbo.Points", "WindowsStoreLink", c => c.String());
            AddColumn("dbo.Points", "WindowsStorePrice", c => c.Double());
            AddColumn("dbo.Points", "AppStoreLink", c => c.String());
            AddColumn("dbo.Points", "AppStorePrice", c => c.Double());
            AddColumn("dbo.Points", "GooglePlayLink", c => c.String());
            AddColumn("dbo.Points", "GooglePlayPrice", c => c.Double());
            AddColumn("dbo.Points", "GogLink", c => c.String());
            AddColumn("dbo.Points", "GogPrice", c => c.Double());
            AddColumn("dbo.Points", "BattleNetLink", c => c.String());
            AddColumn("dbo.Points", "BattleNetPrice", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Points", "BattleNetPrice");
            DropColumn("dbo.Points", "BattleNetLink");
            DropColumn("dbo.Points", "GogPrice");
            DropColumn("dbo.Points", "GogLink");
            DropColumn("dbo.Points", "GooglePlayPrice");
            DropColumn("dbo.Points", "GooglePlayLink");
            DropColumn("dbo.Points", "AppStorePrice");
            DropColumn("dbo.Points", "AppStoreLink");
            DropColumn("dbo.Points", "WindowsStorePrice");
            DropColumn("dbo.Points", "WindowsStoreLink");
            DropColumn("dbo.Points", "OriginPrice");
            DropColumn("dbo.Points", "OriginLink");
        }
    }
}
