namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointThemeStyle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Points", "Logo", c => c.String(nullable: false, maxLength: 256));
            AddColumn("dbo.Points", "ThemeColor", c => c.String(nullable: false, maxLength: 7));
            AddColumn("dbo.Points", "LightThemeColor", c => c.String(nullable: false, maxLength: 7));
            DropColumn("dbo.Points", "EmblemImage");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Points", "EmblemImage", c => c.String(nullable: false, maxLength: 256));
            DropColumn("dbo.Points", "LightThemeColor");
            DropColumn("dbo.Points", "ThemeColor");
            DropColumn("dbo.Points", "Logo");
        }
    }
}
