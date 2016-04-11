namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserLastVisitTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "LastVisitTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KeylolUsers", "LastVisitTime");
        }
    }
}
