namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveLastUpdateTime : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.KeylolUsers", "LastGameUpdateTime");
            DropColumn("dbo.KeylolUsers", "LastGameUpdateSucceed");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KeylolUsers", "LastGameUpdateSucceed", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "LastGameUpdateTime", c => c.DateTime(nullable: false));
        }
    }
}
