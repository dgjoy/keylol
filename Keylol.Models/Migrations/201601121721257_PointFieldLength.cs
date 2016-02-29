namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointFieldLength : DbMigration
    {
        public override void Up()
        {
            this.DeleteDefaultContraint("dbo.NormalPoints", "BackgroundImage");
            this.DeleteDefaultContraint("dbo.NormalPoints", "AvatarImage");
            this.DeleteDefaultContraint("dbo.NormalPoints", "EnglishAliases");
            this.DeleteDefaultContraint("dbo.NormalPoints", "ChineseAliases");
            this.DeleteDefaultContraint("dbo.NormalPoints", "DisplayAliases");
            this.DeleteDefaultContraint("dbo.NormalPoints", "CoverImage");
            this.DeleteDefaultContraint("dbo.ProfilePoints", "BackgroundImage");
            AlterColumn("dbo.NormalPoints", "BackgroundImage", c => c.String(nullable: false, maxLength: 256, defaultValue: ""));
            AlterColumn("dbo.NormalPoints", "AvatarImage", c => c.String(nullable: false, maxLength: 256, defaultValue: ""));
            AlterColumn("dbo.NormalPoints", "EnglishAliases", c => c.String(nullable: false, maxLength: 256, defaultValue: ""));
            AlterColumn("dbo.NormalPoints", "ChineseAliases", c => c.String(nullable: false, maxLength: 256, defaultValue: ""));
            AlterColumn("dbo.NormalPoints", "DisplayAliases", c => c.String(nullable: false, maxLength: 256, defaultValue: ""));
            AlterColumn("dbo.NormalPoints", "CoverImage", c => c.String(nullable: false, maxLength: 256, defaultValue: ""));
            AlterColumn("dbo.ProfilePoints", "BackgroundImage", c => c.String(nullable: false, maxLength: 256, defaultValue: ""));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProfilePoints", "BackgroundImage", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.NormalPoints", "CoverImage", c => c.String(nullable: false));
            AlterColumn("dbo.NormalPoints", "DisplayAliases", c => c.String(nullable: false));
            AlterColumn("dbo.NormalPoints", "ChineseAliases", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.NormalPoints", "EnglishAliases", c => c.String(nullable: false, maxLength: 32));
            AlterColumn("dbo.NormalPoints", "AvatarImage", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.NormalPoints", "BackgroundImage", c => c.String(nullable: false, maxLength: 64));
        }
    }
}
