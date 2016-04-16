namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamBindingTokenBotIdOptional : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.SteamBindingTokens", new[] { "BotId" });
            AlterColumn("dbo.SteamBindingTokens", "BotId", c => c.String(maxLength: 128));
            CreateIndex("dbo.SteamBindingTokens", "BotId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SteamBindingTokens", new[] { "BotId" });
            AlterColumn("dbo.SteamBindingTokens", "BotId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.SteamBindingTokens", "BotId");
        }
    }
}
