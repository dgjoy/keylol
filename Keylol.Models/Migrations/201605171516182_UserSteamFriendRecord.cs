namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserSteamFriendRecord : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UserGameRecords", newName: "UserSteamGameRecords");
            CreateTable(
                "dbo.UserSteamFriendRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        FriendSteamId = c.String(nullable: false, maxLength: 128),
                        FriendSince = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.FriendSteamId);
            
            AddColumn("dbo.Points", "InAppPurchases", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Points", "InAppPurchases");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserSteamFriendRecords", "UserId", "dbo.KeylolUsers");
            DropIndex("dbo.UserSteamFriendRecords", new[] { "FriendSteamId" });
            DropIndex("dbo.UserSteamFriendRecords", new[] { "UserId" });
            DropIndex("dbo.Points", new[] { "InAppPurchases" });
            DropColumn("dbo.Points", "InAppPurchases");
            DropTable("dbo.UserSteamFriendRecords");
            RenameTable(name: "dbo.UserSteamGameRecords", newName: "UserGameRecords");
        }
    }
}
