namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Message : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CreateTime = c.DateTime(nullable: false),
                        ReceiverId = c.String(nullable: false, maxLength: 128),
                        OperatorId = c.String(nullable: false, maxLength: 128),
                        Unread = c.Boolean(nullable: false),
                        ArticleId = c.String(maxLength: 128),
                        Reasons = c.String(),
                        ArticleId1 = c.String(maxLength: 128),
                        ArticleId2 = c.String(maxLength: 128),
                        ArticleId3 = c.String(maxLength: 128),
                        ArticleId4 = c.String(maxLength: 128),
                        Reasons1 = c.String(),
                        ArticleId5 = c.String(maxLength: 128),
                        CommentId = c.String(maxLength: 128),
                        Reasons2 = c.String(),
                        CommentId1 = c.String(maxLength: 128),
                        CommentId2 = c.String(maxLength: 128),
                        CommentId3 = c.String(maxLength: 128),
                        CommentId4 = c.String(maxLength: 128),
                        Reasons3 = c.String(),
                        CommentId5 = c.String(maxLength: 128),
                        ArticleId6 = c.String(maxLength: 128),
                        Reasons4 = c.String(),
                        ArticleId7 = c.String(maxLength: 128),
                        ArticleId8 = c.String(maxLength: 128),
                        ArticleId9 = c.String(maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Articles", t => t.ArticleId)
                .ForeignKey("dbo.Articles", t => t.ArticleId1)
                .ForeignKey("dbo.Articles", t => t.ArticleId2)
                .ForeignKey("dbo.Articles", t => t.ArticleId3)
                .ForeignKey("dbo.Articles", t => t.ArticleId4)
                .ForeignKey("dbo.Articles", t => t.ArticleId5)
                .ForeignKey("dbo.Comments", t => t.CommentId)
                .ForeignKey("dbo.Comments", t => t.CommentId1)
                .ForeignKey("dbo.Comments", t => t.CommentId2)
                .ForeignKey("dbo.Comments", t => t.CommentId3)
                .ForeignKey("dbo.Comments", t => t.CommentId4)
                .ForeignKey("dbo.Comments", t => t.CommentId5)
                .ForeignKey("dbo.KeylolUsers", t => t.OperatorId)
                .ForeignKey("dbo.KeylolUsers", t => t.ReceiverId)
                .ForeignKey("dbo.Articles", t => t.ArticleId6)
                .ForeignKey("dbo.Articles", t => t.ArticleId7)
                .ForeignKey("dbo.Articles", t => t.ArticleId8)
                .ForeignKey("dbo.Articles", t => t.ArticleId9)
                .Index(t => t.CreateTime)
                .Index(t => t.ReceiverId)
                .Index(t => t.OperatorId)
                .Index(t => t.ArticleId)
                .Index(t => t.ArticleId1)
                .Index(t => t.ArticleId2)
                .Index(t => t.ArticleId3)
                .Index(t => t.ArticleId4)
                .Index(t => t.ArticleId5)
                .Index(t => t.CommentId)
                .Index(t => t.CommentId1)
                .Index(t => t.CommentId2)
                .Index(t => t.CommentId3)
                .Index(t => t.CommentId4)
                .Index(t => t.CommentId5)
                .Index(t => t.ArticleId6)
                .Index(t => t.ArticleId7)
                .Index(t => t.ArticleId8)
                .Index(t => t.ArticleId9);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Messages", "ArticleId9", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId8", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId7", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId6", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ReceiverId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Messages", "OperatorId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Messages", "CommentId5", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId4", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId3", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId2", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId1", "dbo.Comments");
            DropForeignKey("dbo.Messages", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.Messages", "ArticleId5", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId4", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId3", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId2", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId1", "dbo.Articles");
            DropForeignKey("dbo.Messages", "ArticleId", "dbo.Articles");
            DropIndex("dbo.Messages", new[] { "ArticleId9" });
            DropIndex("dbo.Messages", new[] { "ArticleId8" });
            DropIndex("dbo.Messages", new[] { "ArticleId7" });
            DropIndex("dbo.Messages", new[] { "ArticleId6" });
            DropIndex("dbo.Messages", new[] { "CommentId5" });
            DropIndex("dbo.Messages", new[] { "CommentId4" });
            DropIndex("dbo.Messages", new[] { "CommentId3" });
            DropIndex("dbo.Messages", new[] { "CommentId2" });
            DropIndex("dbo.Messages", new[] { "CommentId1" });
            DropIndex("dbo.Messages", new[] { "CommentId" });
            DropIndex("dbo.Messages", new[] { "ArticleId5" });
            DropIndex("dbo.Messages", new[] { "ArticleId4" });
            DropIndex("dbo.Messages", new[] { "ArticleId3" });
            DropIndex("dbo.Messages", new[] { "ArticleId2" });
            DropIndex("dbo.Messages", new[] { "ArticleId1" });
            DropIndex("dbo.Messages", new[] { "ArticleId" });
            DropIndex("dbo.Messages", new[] { "OperatorId" });
            DropIndex("dbo.Messages", new[] { "ReceiverId" });
            DropIndex("dbo.Messages", new[] { "CreateTime" });
            DropTable("dbo.Messages");
        }
    }
}
