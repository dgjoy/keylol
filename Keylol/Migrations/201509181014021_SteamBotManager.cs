namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamBotManager : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SteamBots",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        SteamUserName = c.String(nullable: false, maxLength: 64),
                        SteamPassword = c.String(nullable: false, maxLength: 64),
                        SteamId = c.Long(),
                        FriendCount = c.Int(nullable: false),
                        FriendUpperLimit = c.Int(nullable: false),
                        Online = c.Boolean(nullable: false),
                        Manager_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SteamBotManagers", t => t.Manager_Id)
                .Index(t => t.SteamUserName, unique: true)
                .Index(t => t.Manager_Id);
            
            CreateTable(
                "dbo.SteamBotManagers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ClientId = c.String(nullable: false, maxLength: 64),
                        ClientSecret = c.String(nullable: false, maxLength: 64),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.ClientId, unique: true);
            
            AddColumn("dbo.KeylolUsers", "SteamBot_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.SteamBindingTokens", "BrowserConnectionId", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.SteamBindingTokens", "Bot_Id", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.SteamLoginTokens", "BrowserConnectionId", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.SteamLoginTokens", "SteamId", c => c.Long());
            CreateIndex("dbo.KeylolUsers", "SteamId");
            CreateIndex("dbo.KeylolUsers", "SteamBot_Id");
            CreateIndex("dbo.SteamBindingTokens", "BrowserConnectionId");
            CreateIndex("dbo.SteamBindingTokens", "Bot_Id");
            CreateIndex("dbo.SteamLoginTokens", "BrowserConnectionId");
            AddForeignKey("dbo.SteamBindingTokens", "Bot_Id", "dbo.SteamBots", "Id");
            AddForeignKey("dbo.KeylolUsers", "SteamBot_Id", "dbo.SteamBots", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.KeylolUsers", "SteamBot_Id", "dbo.SteamBots");
            DropForeignKey("dbo.SteamBots", "Manager_Id", "dbo.SteamBotManagers");
            DropForeignKey("dbo.SteamBindingTokens", "Bot_Id", "dbo.SteamBots");
            DropIndex("dbo.SteamLoginTokens", new[] { "BrowserConnectionId" });
            DropIndex("dbo.SteamBotManagers", new[] { "ClientId" });
            DropIndex("dbo.SteamBindingTokens", new[] { "Bot_Id" });
            DropIndex("dbo.SteamBindingTokens", new[] { "BrowserConnectionId" });
            DropIndex("dbo.SteamBots", new[] { "Manager_Id" });
            DropIndex("dbo.SteamBots", new[] { "SteamUserName" });
            DropIndex("dbo.KeylolUsers", new[] { "SteamBot_Id" });
            DropIndex("dbo.KeylolUsers", new[] { "SteamId" });
            DropColumn("dbo.SteamLoginTokens", "SteamId");
            DropColumn("dbo.SteamLoginTokens", "BrowserConnectionId");
            DropColumn("dbo.SteamBindingTokens", "Bot_Id");
            DropColumn("dbo.SteamBindingTokens", "BrowserConnectionId");
            DropColumn("dbo.KeylolUsers", "SteamBot_Id");
            DropTable("dbo.SteamBotManagers");
            DropTable("dbo.SteamBots");
        }
    }
}
