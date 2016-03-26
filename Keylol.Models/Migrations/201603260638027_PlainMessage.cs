namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlainMessage : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Messages", "ArticleId1", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId2", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId3", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId4", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId5", "dbo.Articles");
            DropForeignKey("dbo.Messages", "CommentId1", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId2", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId3", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId4", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId5", "dbo.Comments");
            DropForeignKey("dbo.Messages", "ArticleId6", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId7", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId8", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId9", "dbo.Articles");
            DropIndex("dbo.Messages", new[] { "ArticleId1" });
            DropIndex("dbo.Messages", new[] { "ArticleId2" });
            DropIndex("dbo.Messages", new[] { "ArticleId3" });
            DropIndex("dbo.Messages", new[] { "ArticleId4" });
            DropIndex("dbo.Messages", new[] { "ArticleId5" });
            DropIndex("dbo.Messages", new[] { "CommentId1" });
            DropIndex("dbo.Messages", new[] { "CommentId2" });
            DropIndex("dbo.Messages", new[] { "CommentId3" });
            DropIndex("dbo.Messages", new[] { "CommentId4" });
            DropIndex("dbo.Messages", new[] { "CommentId5" });
            DropIndex("dbo.Messages", new[] { "ArticleId6" });
            DropIndex("dbo.Messages", new[] { "ArticleId7" });
            DropIndex("dbo.Messages", new[] { "ArticleId8" });
            DropIndex("dbo.Messages", new[] { "ArticleId9" });
            AddColumn("dbo.Messages", "Type", c => c.Int(nullable: false));
            AlterColumn("dbo.Messages", "Reasons", c => c.String(nullable: false));
            CreateIndex("dbo.Messages", "Unread");
            CreateIndex("dbo.Messages", "Type");
            DropColumn("dbo.Messages", "ArticleId1");
            DropColumn("dbo.Messages", "ArticleId2");
            DropColumn("dbo.Messages", "ArticleId3");
            DropColumn("dbo.Messages", "ArticleId4");
            DropColumn("dbo.Messages", "Reasons1");
            DropColumn("dbo.Messages", "ArticleId5");
            DropColumn("dbo.Messages", "Reasons2");
            DropColumn("dbo.Messages", "CommentId1");
            DropColumn("dbo.Messages", "CommentId2");
            DropColumn("dbo.Messages", "CommentId3");
            DropColumn("dbo.Messages", "CommentId4");
            DropColumn("dbo.Messages", "Reasons3");
            DropColumn("dbo.Messages", "CommentId5");
            DropColumn("dbo.Messages", "ArticleId6");
            DropColumn("dbo.Messages", "Reasons4");
            DropColumn("dbo.Messages", "ArticleId7");
            DropColumn("dbo.Messages", "ArticleId8");
            DropColumn("dbo.Messages", "ArticleId9");
            DropColumn("dbo.Messages", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Messages", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Messages", "ArticleId9", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "ArticleId8", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "ArticleId7", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "Reasons4", c => c.String());
            AddColumn("dbo.Messages", "ArticleId6", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "CommentId5", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "Reasons3", c => c.String());
            AddColumn("dbo.Messages", "CommentId4", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "CommentId3", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "CommentId2", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "CommentId1", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "Reasons2", c => c.String());
            AddColumn("dbo.Messages", "ArticleId5", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "Reasons1", c => c.String());
            AddColumn("dbo.Messages", "ArticleId4", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "ArticleId3", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "ArticleId2", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "ArticleId1", c => c.String(maxLength: 128));
            DropIndex("dbo.Messages", new[] { "Type" });
            DropIndex("dbo.Messages", new[] { "Unread" });
            AlterColumn("dbo.Messages", "Reasons", c => c.String());
            DropColumn("dbo.Messages", "Type");
            CreateIndex("dbo.Messages", "ArticleId9");
            CreateIndex("dbo.Messages", "ArticleId8");
            CreateIndex("dbo.Messages", "ArticleId7");
            CreateIndex("dbo.Messages", "ArticleId6");
            CreateIndex("dbo.Messages", "CommentId5");
            CreateIndex("dbo.Messages", "CommentId4");
            CreateIndex("dbo.Messages", "CommentId3");
            CreateIndex("dbo.Messages", "CommentId2");
            CreateIndex("dbo.Messages", "CommentId1");
            CreateIndex("dbo.Messages", "ArticleId5");
            CreateIndex("dbo.Messages", "ArticleId4");
            CreateIndex("dbo.Messages", "ArticleId3");
            CreateIndex("dbo.Messages", "ArticleId2");
            CreateIndex("dbo.Messages", "ArticleId1");
            AddForeignKey("dbo.Messages", "ArticleId9", "dbo.Articles", "Id");
            AddForeignKey("dbo.Messages", "ArticleId8", "dbo.Articles", "Id");
            AddForeignKey("dbo.Messages", "ArticleId7", "dbo.Articles", "Id");
            AddForeignKey("dbo.Messages", "ArticleId6", "dbo.Articles", "Id");
            AddForeignKey("dbo.Messages", "CommentId5", "dbo.Comments", "Id");
            AddForeignKey("dbo.Messages", "CommentId4", "dbo.Comments", "Id");
            AddForeignKey("dbo.Messages", "CommentId3", "dbo.Comments", "Id");
            AddForeignKey("dbo.Messages", "CommentId2", "dbo.Comments", "Id");
            AddForeignKey("dbo.Messages", "CommentId1", "dbo.Comments", "Id");
            AddForeignKey("dbo.Messages", "ArticleId5", "dbo.Articles", "Id");
            AddForeignKey("dbo.Messages", "ArticleId4", "dbo.Articles", "Id");
            AddForeignKey("dbo.Messages", "ArticleId3", "dbo.Articles", "Id");
            AddForeignKey("dbo.Messages", "ArticleId2", "dbo.Articles", "Id");
            AddForeignKey("dbo.Messages", "ArticleId1", "dbo.Articles", "Id");
        }
    }
}
