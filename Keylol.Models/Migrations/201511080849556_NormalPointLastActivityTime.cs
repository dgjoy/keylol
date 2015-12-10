namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NormalPointLastActivityTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NormalPoints", "LastActivityTime", c => c.DateTime(nullable: false));
            CreateIndex("dbo.KeylolUsers", "RegisterTime");
            CreateIndex("dbo.NormalPoints", "LastActivityTime");
            DropColumn("dbo.KeylolUsers", "SteamBindingLockEnabled");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KeylolUsers", "SteamBindingLockEnabled", c => c.Boolean(nullable: false));
            DropIndex("dbo.NormalPoints", new[] { "LastActivityTime" });
            DropIndex("dbo.KeylolUsers", new[] { "RegisterTime" });
            DropColumn("dbo.NormalPoints", "LastActivityTime");
        }
    }
}
