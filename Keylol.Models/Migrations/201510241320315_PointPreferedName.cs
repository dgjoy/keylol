namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointPreferedName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NormalPoints", "PreferedName", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NormalPoints", "PreferedName");
        }
    }
}
