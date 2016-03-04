namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AutoSubscriptionDisplayOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AutoSubscriptions", "DisplayOrder", c => c.Int(nullable: false));
            CreateIndex("dbo.AutoSubscriptions", "DisplayOrder");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AutoSubscriptions", new[] { "DisplayOrder" });
            DropColumn("dbo.AutoSubscriptions", "DisplayOrder");
        }
    }
}
