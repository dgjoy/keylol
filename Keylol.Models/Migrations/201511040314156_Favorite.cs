namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Favorite : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Favorites",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    UserId = c.String(nullable: false, maxLength: 128),
                    PointId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.PointId);

        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Favorites", "UserId", "dbo.KeylolUsers");
            DropIndex("dbo.Favorites", new[] { "UserId", "PointId" });
            DropTable("dbo.Favorites");
        }
    }
}
