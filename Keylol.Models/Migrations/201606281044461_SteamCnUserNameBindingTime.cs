namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamCnUserNameBindingTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "SteamCnUserName", c => c.String(nullable: false, maxLength: 64));
            AddColumn("dbo.KeylolUsers", "SteamCnBindingTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KeylolUsers", "SteamCnBindingTime");
            DropColumn("dbo.KeylolUsers", "SteamCnUserName");
        }
    }
}
