namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveSteamBotManager : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SteamBots", "Manager_Id", "dbo.SteamBotManagers");
            DropIndex("dbo.SteamBots", new[] { "Manager_Id" });
            DropIndex("dbo.SteamBotManagers", new[] { "ClientId" });
            AddColumn("dbo.SteamBots", "SessionId", c => c.String());
            DropColumn("dbo.SteamBots", "Manager_Id");
            DropTable("dbo.SteamBotManagers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SteamBotManagers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ClientId = c.String(nullable: false, maxLength: 64),
                        ClientSecret = c.String(nullable: false, maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.SteamBots", "Manager_Id", c => c.String(maxLength: 128));
            DropColumn("dbo.SteamBots", "SessionId");
            CreateIndex("dbo.SteamBotManagers", "ClientId", unique: true);
            CreateIndex("dbo.SteamBots", "Manager_Id");
            AddForeignKey("dbo.SteamBots", "Manager_Id", "dbo.SteamBotManagers", "Id");
        }
    }
}
