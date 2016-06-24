namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleCommentReplyToComment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ArticleComments", "ReplyToCommentId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ArticleComments", "ReplyToCommentId");
            AddForeignKey("dbo.ArticleComments", "ReplyToCommentId", "dbo.ArticleComments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ArticleComments", "ReplyToCommentId", "dbo.ArticleComments");
            DropIndex("dbo.ArticleComments", new[] { "ReplyToCommentId" });
            DropColumn("dbo.ArticleComments", "ReplyToCommentId");
        }
    }
}
