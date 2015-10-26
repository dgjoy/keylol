namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LikeBackout : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Likes", "Backout", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Likes", "Backout");
        }
    }
}
