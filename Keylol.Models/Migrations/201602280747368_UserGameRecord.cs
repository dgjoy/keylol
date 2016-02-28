namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserGameRecord : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserGameRecords",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        SteamAppId = c.Int(nullable: false),
                        TotalPlayedTime = c.Double(nullable: false),
                        RecentPlayedTime = c.Double(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.SteamAppId)
                .Index(t => t.DisplayOrder);
            
            AddColumn("dbo.KeylolUsers", "LastGameUpdateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.KeylolUsers", "LastGameUpdateSucceed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserGameRecords", "UserId", "dbo.KeylolUsers");
            DropIndex("dbo.UserGameRecords", new[] { "DisplayOrder" });
            DropIndex("dbo.UserGameRecords", new[] { "SteamAppId" });
            DropIndex("dbo.UserGameRecords", new[] { "UserId" });
            DropColumn("dbo.KeylolUsers", "LastGameUpdateSucceed");
            DropColumn("dbo.KeylolUsers", "LastGameUpdateTime");
            DropTable("dbo.UserGameRecords");
        }
    }
}
