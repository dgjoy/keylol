namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserGameRecordLastPlayTime : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.UserGameRecords", new[] { "DisplayOrder" });
            AddColumn("dbo.UserGameRecords", "LastPlayTime", c => c.DateTime(nullable: false));
            CreateIndex("dbo.UserGameRecords", "LastPlayTime");
            DropColumn("dbo.UserGameRecords", "RecentPlayedTime");
            DropColumn("dbo.UserGameRecords", "DisplayOrder");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserGameRecords", "DisplayOrder", c => c.Int(nullable: false));
            AddColumn("dbo.UserGameRecords", "RecentPlayedTime", c => c.Double(nullable: false));
            DropIndex("dbo.UserGameRecords", new[] { "LastPlayTime" });
            DropColumn("dbo.UserGameRecords", "LastPlayTime");
            CreateIndex("dbo.UserGameRecords", "DisplayOrder");
        }
    }
}
