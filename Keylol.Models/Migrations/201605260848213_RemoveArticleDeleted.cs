namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveArticleDeleted : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Activities", "Deleted");
            DropColumn("dbo.ActivityComments", "Deleted");
            DropColumn("dbo.ArticleComments", "Deleted");
            DropColumn("dbo.Articles", "Deleted");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Articles", "Deleted", c => c.Int(nullable: false));
            AddColumn("dbo.ArticleComments", "Deleted", c => c.Int(nullable: false));
            AddColumn("dbo.ActivityComments", "Deleted", c => c.Int(nullable: false));
            AddColumn("dbo.Activities", "Deleted", c => c.Int(nullable: false));
        }
    }
}
