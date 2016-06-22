namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleCommentRowVersion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ArticleComments", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ArticleComments", "RowVersion");
        }
    }
}
