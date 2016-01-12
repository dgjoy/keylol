namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointNameInStore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NormalPoints", "NameInSteamStore", c => c.String(nullable: false, maxLength: 150));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NormalPoints", "NameInSteamStore");
        }
    }
}
