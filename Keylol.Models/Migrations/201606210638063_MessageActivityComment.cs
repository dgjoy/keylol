namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MessageActivityComment : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Messages", name: "CommentId", newName: "ArticleCommentId");
            RenameIndex(table: "dbo.Messages", name: "IX_CommentId", newName: "IX_ArticleCommentId");
            AddColumn("dbo.Messages", "ActivityCommentId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Messages", "ActivityCommentId");
            AddForeignKey("dbo.Messages", "ActivityCommentId", "dbo.ActivityComments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Messages", "ActivityCommentId", "dbo.ActivityComments");
            DropIndex("dbo.Messages", new[] { "ActivityCommentId" });
            DropColumn("dbo.Messages", "ActivityCommentId");
            RenameIndex(table: "dbo.Messages", name: "IX_ArticleCommentId", newName: "IX_CommentId");
            RenameColumn(table: "dbo.Messages", name: "ArticleCommentId", newName: "CommentId");
        }
    }
}
