namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveDeprecatedProperties : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PointAssociations", "ToPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.PointAssociations", "ByPoint_Id", "dbo.NormalPoints");
            DropIndex("dbo.PointAssociations", new[] { "ToPoint_Id" });
            DropIndex("dbo.PointAssociations", new[] { "ByPoint_Id" });
            DropColumn("dbo.NormalPoints", "StoreLink");
            DropTable("dbo.PointAssociations");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PointAssociations",
                c => new
                    {
                        ToPoint_Id = c.String(nullable: false, maxLength: 128),
                        ByPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ToPoint_Id, t.ByPoint_Id });
            
            AddColumn("dbo.NormalPoints", "StoreLink", c => c.String(nullable: false, maxLength: 512));
            CreateIndex("dbo.PointAssociations", "ByPoint_Id");
            CreateIndex("dbo.PointAssociations", "ToPoint_Id");
            AddForeignKey("dbo.PointAssociations", "ByPoint_Id", "dbo.NormalPoints", "Id");
            AddForeignKey("dbo.PointAssociations", "ToPoint_Id", "dbo.NormalPoints", "Id");
        }
    }
}
