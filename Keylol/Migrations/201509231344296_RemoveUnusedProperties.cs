namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnusedProperties : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SteamBindingTokens", "Consumed");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SteamBindingTokens", "Consumed", c => c.Boolean(nullable: false));
        }
    }
}
