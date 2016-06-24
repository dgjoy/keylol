namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveMajorMinorPlatformRelationship : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Points", "NameInSteamStore");
            Sql("UPDATE [dbo].[PointRelationships] SET Relationship = 6 WHERE Relationship = 7");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Points", "NameInSteamStore", c => c.String(nullable: false, maxLength: 150));
        }
    }
}
