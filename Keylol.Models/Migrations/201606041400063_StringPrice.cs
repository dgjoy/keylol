namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StringPrice : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE dbo.Points SET UplayLink = N'' WHERE UplayLink IS NULL");
            Sql(@"UPDATE dbo.Points SET UplayPrice = N'' WHERE UplayPrice IS NULL");
            Sql(@"UPDATE dbo.Points SET XboxLink = N'' WHERE XboxLink IS NULL");
            Sql(@"UPDATE dbo.Points SET XboxPrice = N'' WHERE XboxPrice IS NULL");
            Sql(@"UPDATE dbo.Points SET PlayStationLink = N'' WHERE PlayStationLink IS NULL");
            Sql(@"UPDATE dbo.Points SET PlayStationPrice = N'' WHERE PlayStationPrice IS NULL");
            Sql(@"UPDATE dbo.Points SET OriginLink = N'' WHERE OriginLink IS NULL");
            Sql(@"UPDATE dbo.Points SET OriginPrice = N'' WHERE OriginPrice IS NULL");
            Sql(@"UPDATE dbo.Points SET WindowsStoreLink = N'' WHERE WindowsStoreLink IS NULL");
            Sql(@"UPDATE dbo.Points SET WindowsStorePrice = N'' WHERE WindowsStorePrice IS NULL");
            Sql(@"UPDATE dbo.Points SET AppStoreLink = N'' WHERE AppStoreLink IS NULL");
            Sql(@"UPDATE dbo.Points SET AppStorePrice = N'' WHERE AppStorePrice IS NULL");
            Sql(@"UPDATE dbo.Points SET GooglePlayLink = N'' WHERE GooglePlayLink IS NULL");
            Sql(@"UPDATE dbo.Points SET GooglePlayPrice = N'' WHERE GooglePlayPrice IS NULL");
            Sql(@"UPDATE dbo.Points SET GogLink = N'' WHERE GogLink IS NULL");
            Sql(@"UPDATE dbo.Points SET GogPrice = N'' WHERE GogPrice IS NULL");
            Sql(@"UPDATE dbo.Points SET BattleNetLink = N'' WHERE BattleNetLink IS NULL");
            Sql(@"UPDATE dbo.Points SET BattleNetPrice = N'' WHERE BattleNetPrice IS NULL");

            AlterColumn("dbo.Points", "UplayLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "UplayPrice", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Points", "XboxLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "XboxPrice", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Points", "PlayStationLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "PlayStationPrice", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Points", "OriginLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "OriginPrice", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Points", "WindowsStoreLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "WindowsStorePrice", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Points", "AppStoreLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "AppStorePrice", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Points", "GooglePlayLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "GooglePlayPrice", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Points", "GogLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "GogPrice", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Points", "BattleNetLink", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Points", "BattleNetPrice", c => c.String(nullable: false, maxLength: 50));

            Sql(@"UPDATE dbo.Points SET UplayPrice = N'' WHERE UplayPrice = N'0'");
            Sql(@"UPDATE dbo.Points SET XboxPrice = N'' WHERE XboxPrice = N'0'");
            Sql(@"UPDATE dbo.Points SET PlayStationPrice = N'' WHERE PlayStationPrice = N'0'");
            Sql(@"UPDATE dbo.Points SET OriginPrice = N'' WHERE OriginPrice = N'0'");
            Sql(@"UPDATE dbo.Points SET WindowsStorePrice = N'' WHERE WindowsStorePrice = N'0'");
            Sql(@"UPDATE dbo.Points SET AppStorePrice = N'' WHERE AppStorePrice = N'0'");
            Sql(@"UPDATE dbo.Points SET GooglePlayPrice = N'' WHERE GooglePlayPrice = N'0'");
            Sql(@"UPDATE dbo.Points SET GogPrice = N'' WHERE GogPrice = N'0'");
            Sql(@"UPDATE dbo.Points SET BattleNetPrice = N'' WHERE BattleNetPrice = N'0'");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Points", "BattleNetPrice", c => c.Double());
            AlterColumn("dbo.Points", "BattleNetLink", c => c.String());
            AlterColumn("dbo.Points", "GogPrice", c => c.Double());
            AlterColumn("dbo.Points", "GogLink", c => c.String());
            AlterColumn("dbo.Points", "GooglePlayPrice", c => c.Double());
            AlterColumn("dbo.Points", "GooglePlayLink", c => c.String());
            AlterColumn("dbo.Points", "AppStorePrice", c => c.Double());
            AlterColumn("dbo.Points", "AppStoreLink", c => c.String());
            AlterColumn("dbo.Points", "WindowsStorePrice", c => c.Double());
            AlterColumn("dbo.Points", "WindowsStoreLink", c => c.String());
            AlterColumn("dbo.Points", "OriginPrice", c => c.Double());
            AlterColumn("dbo.Points", "OriginLink", c => c.String());
            AlterColumn("dbo.Points", "PlayStationPrice", c => c.Double());
            AlterColumn("dbo.Points", "PlayStationLink", c => c.String());
            AlterColumn("dbo.Points", "XboxPrice", c => c.Double());
            AlterColumn("dbo.Points", "XboxLink", c => c.String());
            AlterColumn("dbo.Points", "UplayPrice", c => c.Double());
            AlterColumn("dbo.Points", "UplayLink", c => c.String());
        }
    }
}
