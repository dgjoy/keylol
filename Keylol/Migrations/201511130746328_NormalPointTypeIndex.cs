namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NormalPointTypeIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.NormalPoints", "Type");
        }
        
        public override void Down()
        {
            DropIndex("dbo.NormalPoints", new[] { "Type" });
        }
    }
}
