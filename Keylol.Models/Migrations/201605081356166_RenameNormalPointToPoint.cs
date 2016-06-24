namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameNormalPointToPoint : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.PointStoreNameMappings", name: "NormalPoint_Id", newName: "Point_Id");
            RenameIndex(table: "dbo.PointStoreNameMappings", name: "IX_NormalPoint_Id", newName: "IX_Point_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.PointStoreNameMappings", name: "IX_Point_Id", newName: "IX_NormalPoint_Id");
            RenameColumn(table: "dbo.PointStoreNameMappings", name: "Point_Id", newName: "NormalPoint_Id");
        }
    }
}
