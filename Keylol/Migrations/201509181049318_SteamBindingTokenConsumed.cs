namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamBindingTokenConsumed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SteamBindingTokens", "Consumed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SteamBindingTokens", "Consumed");
        }
    }
}
