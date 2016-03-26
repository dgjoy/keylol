namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOldIgnoreProperties : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Likes", "ReadByTargetUser");
            DropColumn("dbo.Likes", "IgnoredByTargetUser");
            DropColumn("dbo.Comments", "ReadByArticleAuthor");
            DropColumn("dbo.Comments", "IgnoredByArticleAuthor");
            DropColumn("dbo.CommentReplies", "IgnoredByCommentAuthor");
            DropColumn("dbo.CommentReplies", "ReadByCommentAuthor");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CommentReplies", "ReadByCommentAuthor", c => c.Boolean(nullable: false));
            AddColumn("dbo.CommentReplies", "IgnoredByCommentAuthor", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "IgnoredByArticleAuthor", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "ReadByArticleAuthor", c => c.Boolean(nullable: false));
            AddColumn("dbo.Likes", "IgnoredByTargetUser", c => c.Boolean(nullable: false));
            AddColumn("dbo.Likes", "ReadByTargetUser", c => c.Boolean(nullable: false));
        }
    }
}
