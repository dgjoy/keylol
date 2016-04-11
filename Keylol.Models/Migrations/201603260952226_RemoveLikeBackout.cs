namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveLikeBackout : DbMigration
    {
        public override void Up()
        {
            Sql("DELETE FROM [dbo].[Likes] WHERE [Backout] = 'True'");
            DropIndex("dbo.Likes", new[] { "Backout" });
            DropColumn("dbo.Likes", "Backout");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Likes", "Backout", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Likes", "Backout");
        }
    }
}
