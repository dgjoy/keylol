namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserNotifyOnSubscribed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "NotifyOnSubscribed", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnSubscribed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnSubscribed");
            DropColumn("dbo.KeylolUsers", "NotifyOnSubscribed");
        }
    }
}
