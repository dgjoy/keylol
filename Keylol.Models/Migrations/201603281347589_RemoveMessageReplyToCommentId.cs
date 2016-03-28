namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveMessageReplyToCommentId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Messages", "ReplyToCommentId", "dbo.Comments");
            DropIndex("dbo.Messages", new[] { "ReplyToCommentId" });
            DropColumn("dbo.Messages", "ReplyToCommentId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Messages", "ReplyToCommentId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Messages", "ReplyToCommentId");
            AddForeignKey("dbo.Messages", "ReplyToCommentId", "dbo.Comments", "Id");
        }
    }
}
