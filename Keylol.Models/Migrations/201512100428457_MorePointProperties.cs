namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MorePointProperties : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameDeveloperPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        DeveloperPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.DeveloperPoint_Id })
                .ForeignKey("dbo.NormalPoints", t => t.GamePoint_Id)
                .ForeignKey("dbo.NormalPoints", t => t.DeveloperPoint_Id)
                .Index(t => t.GamePoint_Id)
                .Index(t => t.DeveloperPoint_Id);
            
            CreateTable(
                "dbo.GameGenrePointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        GenrePoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.GenrePoint_Id })
                .ForeignKey("dbo.NormalPoints", t => t.GamePoint_Id)
                .ForeignKey("dbo.NormalPoints", t => t.GenrePoint_Id)
                .Index(t => t.GamePoint_Id)
                .Index(t => t.GenrePoint_Id);
            
            CreateTable(
                "dbo.GameMajorPlatformPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        MajorPlatformPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.MajorPlatformPoint_Id })
                .ForeignKey("dbo.NormalPoints", t => t.GamePoint_Id)
                .ForeignKey("dbo.NormalPoints", t => t.MajorPlatformPoint_Id)
                .Index(t => t.GamePoint_Id)
                .Index(t => t.MajorPlatformPoint_Id);
            
            CreateTable(
                "dbo.GameMinorPlatformPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        MinorPlatformPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.MinorPlatformPoint_Id })
                .ForeignKey("dbo.NormalPoints", t => t.GamePoint_Id)
                .ForeignKey("dbo.NormalPoints", t => t.MinorPlatformPoint_Id)
                .Index(t => t.GamePoint_Id)
                .Index(t => t.MinorPlatformPoint_Id);
            
            CreateTable(
                "dbo.GamePublisherPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        PublisherPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.PublisherPoint_Id })
                .ForeignKey("dbo.NormalPoints", t => t.GamePoint_Id)
                .ForeignKey("dbo.NormalPoints", t => t.PublisherPoint_Id)
                .Index(t => t.GamePoint_Id)
                .Index(t => t.PublisherPoint_Id);
            
            CreateTable(
                "dbo.GameTagPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        TagPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.TagPoint_Id })
                .ForeignKey("dbo.NormalPoints", t => t.GamePoint_Id)
                .ForeignKey("dbo.NormalPoints", t => t.TagPoint_Id)
                .Index(t => t.GamePoint_Id)
                .Index(t => t.TagPoint_Id);
            
            AddColumn("dbo.NormalPoints", "Description", c => c.String(nullable: false));
            AddColumn("dbo.NormalPoints", "SteamAppId", c => c.Int(nullable: false));
            AddColumn("dbo.NormalPoints", "DisplayAliases", c => c.String(nullable: false));
            AddColumn("dbo.NormalPoints", "ReleaseDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.NormalPoints", "CoverImage", c => c.String(nullable: false));
            CreateIndex("dbo.NormalPoints", "SteamAppId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameTagPointAssociations", "TagPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameTagPointAssociations", "GamePoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GamePublisherPointAssociations", "PublisherPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GamePublisherPointAssociations", "GamePoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameMinorPlatformPointAssociations", "MinorPlatformPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameMinorPlatformPointAssociations", "GamePoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameMajorPlatformPointAssociations", "MajorPlatformPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameMajorPlatformPointAssociations", "GamePoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameGenrePointAssociations", "GenrePoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameGenrePointAssociations", "GamePoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameDeveloperPointAssociations", "DeveloperPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameDeveloperPointAssociations", "GamePoint_Id", "dbo.NormalPoints");
            DropIndex("dbo.NormalPoints", new[] { "SteamAppId" });
            DropIndex("dbo.GameTagPointAssociations", new[] { "TagPoint_Id" });
            DropIndex("dbo.GameTagPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GamePublisherPointAssociations", new[] { "PublisherPoint_Id" });
            DropIndex("dbo.GamePublisherPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameMinorPlatformPointAssociations", new[] { "MinorPlatformPoint_Id" });
            DropIndex("dbo.GameMinorPlatformPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameMajorPlatformPointAssociations", new[] { "MajorPlatformPoint_Id" });
            DropIndex("dbo.GameMajorPlatformPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameGenrePointAssociations", new[] { "GenrePoint_Id" });
            DropIndex("dbo.GameGenrePointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameDeveloperPointAssociations", new[] { "DeveloperPoint_Id" });
            DropIndex("dbo.GameDeveloperPointAssociations", new[] { "GamePoint_Id" });
            DropColumn("dbo.NormalPoints", "CoverImage");
            DropColumn("dbo.NormalPoints", "ReleaseDate");
            DropColumn("dbo.NormalPoints", "DisplayAliases");
            DropColumn("dbo.NormalPoints", "SteamAppId");
            DropColumn("dbo.NormalPoints", "Description");
            DropTable("dbo.GameTagPointAssociations");
            DropTable("dbo.GamePublisherPointAssociations");
            DropTable("dbo.GameMinorPlatformPointAssociations");
            DropTable("dbo.GameMajorPlatformPointAssociations");
            DropTable("dbo.GameGenrePointAssociations");
            DropTable("dbo.GameDeveloperPointAssociations");
        }
    }
}
