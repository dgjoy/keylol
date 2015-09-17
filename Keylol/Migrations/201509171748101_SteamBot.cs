namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamBot : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SteamBindingTokens",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 8),
                        SteamId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            CreateTable(
                "dbo.SteamLoginTokens",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Code = c.String(nullable: false, maxLength: 4),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            AddColumn("dbo.KeylolUsers", "SteamId", c => c.Long());
            AddColumn("dbo.KeylolUsers", "SteamBindingTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.KeylolUsers", "SteamBindingLockEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropIndex("dbo.SteamLoginTokens", new[] { "Code" });
            DropIndex("dbo.SteamBindingTokens", new[] { "Code" });
            DropColumn("dbo.KeylolUsers", "SteamBindingLockEnabled");
            DropColumn("dbo.KeylolUsers", "SteamBindingTime");
            DropColumn("dbo.KeylolUsers", "SteamId");
            DropTable("dbo.SteamLoginTokens");
            DropTable("dbo.SteamBindingTokens");
        }
    }
}
