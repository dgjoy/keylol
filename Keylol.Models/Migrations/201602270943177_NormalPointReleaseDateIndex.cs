namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NormalPointReleaseDateIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.NormalPoints", "ReleaseDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.NormalPoints", new[] { "ReleaseDate" });
        }
    }
}
