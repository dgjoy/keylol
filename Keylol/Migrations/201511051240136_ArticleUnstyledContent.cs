namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleUnstyledContent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Entries", "ThumbnailImage", c => c.String(maxLength: 1024));
            AddColumn("dbo.Entries", "UnstyledContent", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Entries", "UnstyledContent");
            DropColumn("dbo.Entries", "ThumbnailImage");
        }
    }
}
