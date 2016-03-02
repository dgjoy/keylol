namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamStoreNames : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SteamStoreNames",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.PointStoreNameMappings",
                c => new
                    {
                        NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                        SteamStoreName_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.NormalPoint_Id, t.SteamStoreName_Id })
                .ForeignKey("dbo.NormalPoints", t => t.NormalPoint_Id)
                .ForeignKey("dbo.SteamStoreNames", t => t.SteamStoreName_Id)
                .Index(t => t.NormalPoint_Id)
                .Index(t => t.SteamStoreName_Id);

        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PointStoreNameMappings", "SteamStoreName_Id", "dbo.SteamStoreNames");
            DropForeignKey("dbo.PointStoreNameMappings", "NormalPoint_Id", "dbo.NormalPoints");
            DropIndex("dbo.PointStoreNameMappings", new[] { "SteamStoreName_Id" });
            DropIndex("dbo.PointStoreNameMappings", new[] { "NormalPoint_Id" });
            DropIndex("dbo.SteamStoreNames", new[] { "Name" });
            DropTable("dbo.PointStoreNameMappings");
            DropTable("dbo.SteamStoreNames");
        }
    }
}
