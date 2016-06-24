namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TwoWeekPlayedTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserGameRecords", "TwoWeekPlayedTime", c => c.Double(nullable: false));
            CreateIndex("dbo.UserGameRecords", "TwoWeekPlayedTime");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserGameRecords", new[] { "TwoWeekPlayedTime" });
            DropColumn("dbo.UserGameRecords", "TwoWeekPlayedTime");
        }
    }
}
