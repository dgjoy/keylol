namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeriesPoint : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameSeriesPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        SeriesPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.SeriesPoint_Id })
                .ForeignKey("dbo.NormalPoints", t => t.GamePoint_Id)
                .ForeignKey("dbo.NormalPoints", t => t.SeriesPoint_Id)
                .Index(t => t.GamePoint_Id)
                .Index(t => t.SeriesPoint_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameSeriesPointAssociations", "SeriesPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.GameSeriesPointAssociations", "GamePoint_Id", "dbo.NormalPoints");
            DropIndex("dbo.GameSeriesPointAssociations", new[] { "SeriesPoint_Id" });
            DropIndex("dbo.GameSeriesPointAssociations", new[] { "GamePoint_Id" });
            DropTable("dbo.GameSeriesPointAssociations");
        }
    }
}
