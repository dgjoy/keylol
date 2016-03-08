namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AutoSubscribeSettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "AutoSubscribeEnabled", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.KeylolUsers", "AutoSubscribeDaySpan", c => c.Int(nullable: false, defaultValue: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KeylolUsers", "AutoSubscribeDaySpan");
            DropColumn("dbo.KeylolUsers", "AutoSubscribeEnabled");
        }
    }
}
