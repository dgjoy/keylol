namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActivityArchived : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityComments", "Archived", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityComments", "Archived");
        }
    }
}
