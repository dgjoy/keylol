namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ReplysNoForeignKey : DbMigration
    {
        public override void Up()
        {
            Sql(@"ALTER TABLE dbo.Replies DROP CONSTRAINT [FK_dbo.CommentReplies_dbo.Comments_CommentId]");
            Sql(@"ALTER TABLE dbo.Replies DROP CONSTRAINT [FK_dbo.CommentReplies_dbo.Comments_ReplyId]");
        }

        public override void Down()
        {
        }
    }
}