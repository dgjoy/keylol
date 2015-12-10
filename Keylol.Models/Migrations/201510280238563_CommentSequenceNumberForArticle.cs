namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommentSequenceNumberForArticle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "SequenceNumberForArticle", c => c.Int(nullable: false));
            CreateIndex("dbo.Comments", "SequenceNumberForArticle");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Comments", new[] { "SequenceNumberForArticle" });
            DropColumn("dbo.Comments", "SequenceNumberForArticle");
        }
    }
}
