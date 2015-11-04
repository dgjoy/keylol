namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IgnoreNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Likes", "IgnoredByTargetUser", c => c.Boolean(nullable: false));
            AddColumn("dbo.Entries", "IgnoreNewLikes", c => c.Boolean());
            AddColumn("dbo.Entries", "IgnoreNewComments", c => c.Boolean());
            Sql("UPDATE [dbo].[Entries] SET [IgnoreNewLikes] = 0, [IgnoreNewComments] = 0 WHERE [Discriminator] = 'Article'");
            AddColumn("dbo.Comments", "IgnoredByArticleAuthor", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "IgnoreNewLikes", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "IgnoreNewComments", c => c.Boolean(nullable: false));
            AddColumn("dbo.CommentReplies", "IgnoredByCommentAuthor", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CommentReplies", "IgnoredByCommentAuthor");
            DropColumn("dbo.Comments", "IgnoreNewComments");
            DropColumn("dbo.Comments", "IgnoreNewLikes");
            DropColumn("dbo.Comments", "IgnoredByArticleAuthor");
            DropColumn("dbo.Entries", "IgnoreNewComments");
            DropColumn("dbo.Entries", "IgnoreNewLikes");
            DropColumn("dbo.Likes", "IgnoredByTargetUser");
        }
    }
}
