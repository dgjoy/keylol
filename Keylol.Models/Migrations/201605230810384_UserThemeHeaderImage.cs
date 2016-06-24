namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserThemeHeaderImage : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.KeylolUsers", "BackgroundImage", "HeaderImage");
            AddColumn("dbo.KeylolUsers", "ThemeColor", c => c.String(nullable: false, maxLength: 7));
            AddColumn("dbo.KeylolUsers", "LightThemeColor", c => c.String(nullable: false, maxLength: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KeylolUsers", "LightThemeColor");
            DropColumn("dbo.KeylolUsers", "ThemeColor");
            RenameColumn("dbo.KeylolUsers", "HeaderImage", "BackgroundImage");
        }
    }
}
