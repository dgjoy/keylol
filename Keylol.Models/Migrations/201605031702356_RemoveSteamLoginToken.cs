namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveSteamLoginToken : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.SteamLoginTokens", new[] { "Code" });
            DropIndex("dbo.SteamLoginTokens", new[] { "BrowserConnectionId" });
            DropIndex("dbo.SteamLoginTokens", new[] { "SteamId" });
            DropTable("dbo.SteamLoginTokens");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SteamLoginTokens",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 4),
                        BrowserConnectionId = c.String(nullable: false, maxLength: 128),
                        SteamId = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.SteamLoginTokens", "SteamId");
            CreateIndex("dbo.SteamLoginTokens", "BrowserConnectionId");
            CreateIndex("dbo.SteamLoginTokens", "Code", unique: true);
        }
    }
}
