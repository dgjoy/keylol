namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleSpotlightTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articles", "SpotlightTime", c => c.DateTime());
            CreateIndex("dbo.Articles", "Archived");
            CreateIndex("dbo.Articles", "Rejected");
            CreateIndex("dbo.Articles", "SpotlightTime");
            DropColumn("dbo.Articles", "Spotlight");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Articles", "Spotlight", c => c.Boolean(nullable: false));
            DropIndex("dbo.Articles", new[] { "SpotlightTime" });
            DropIndex("dbo.Articles", new[] { "Rejected" });
            DropIndex("dbo.Articles", new[] { "Archived" });
            DropColumn("dbo.Articles", "SpotlightTime");
        }
    }
}
