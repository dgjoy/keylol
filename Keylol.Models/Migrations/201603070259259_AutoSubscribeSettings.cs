namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AutoSubscribeSettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "AutoSubscribeEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoSubscribeTimeSpan", c => c.Time(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KeylolUsers", "AutoSubscribeTimeSpan");
            DropColumn("dbo.KeylolUsers", "AutoSubscribeEnabled");
        }
    }
}
