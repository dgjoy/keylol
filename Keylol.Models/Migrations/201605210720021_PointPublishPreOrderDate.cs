namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointPublishPreOrderDate : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Points", new[] { "ReleaseDate" });
            CreateTable(
                "dbo.PointStaffs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PointId = c.String(nullable: false, maxLength: 128),
                        StaffId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Points", t => t.PointId)
                .ForeignKey("dbo.KeylolUsers", t => t.StaffId)
                .Index(t => t.PointId)
                .Index(t => t.StaffId);
            
            AddColumn("dbo.Points", "PublishDate", c => c.DateTime());
            AddColumn("dbo.Points", "PreOrderDate", c => c.DateTime());
            AlterColumn("dbo.Points", "ReleaseDate", c => c.DateTime());
            Sql(@"UPDATE Points SET ReleaseDate = NULL WHERE Type NOT IN (0, 4)");
            CreateIndex("dbo.Points", "ReleaseDate");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PointStaffs", "StaffId", "dbo.KeylolUsers");
            DropForeignKey("dbo.PointStaffs", "PointId", "dbo.Points");
            DropIndex("dbo.PointStaffs", new[] { "StaffId" });
            DropIndex("dbo.PointStaffs", new[] { "PointId" });
            DropIndex("dbo.Points", new[] { "ReleaseDate" });
            AlterColumn("dbo.Points", "ReleaseDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Points", "PreOrderDate");
            DropColumn("dbo.Points", "PublishDate");
            DropTable("dbo.PointStaffs");
            CreateIndex("dbo.Points", "ReleaseDate");
        }
    }
}
