namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AutoSubscriptions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AutoSubscriptions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        NormalPointId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NormalPoints", t => t.NormalPointId)
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.NormalPointId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AutoSubscriptions", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.AutoSubscriptions", "NormalPointId", "dbo.NormalPoints");
            DropIndex("dbo.AutoSubscriptions", new[] { "NormalPointId" });
            DropIndex("dbo.AutoSubscriptions", new[] { "UserId" });
            DropTable("dbo.AutoSubscriptions");
        }
    }
}
