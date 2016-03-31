namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserFreeLike : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "FreeLike", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.KeylolUsers", "FreeLike");
        }
    }
}
