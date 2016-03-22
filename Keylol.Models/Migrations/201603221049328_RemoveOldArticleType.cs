namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOldArticleType : DbMigration
    {
        public override void Up()
        {
            Sql(@"IF object_id(N'[dbo].[FK_dbo.Entries_dbo.ArticleTypes_TypeId]', N'F') IS NOT NULL
                ALTER TABLE [dbo].[Articles] DROP CONSTRAINT [FK_dbo.Entries_dbo.ArticleTypes_TypeId]");
            DropForeignKey("dbo.Articles", "TypeId", "dbo.ArticleTypes");
            DropIndex("dbo.Articles", new[] { "TypeId" });
            DropIndex("dbo.ArticleTypes", new[] { "Name" });
            DropColumn("dbo.Articles", "TypeId");
            DropTable("dbo.ArticleTypes");
            RenameColumn("dbo.Articles", "TypeNew", "Type");
            CreateIndex("dbo.Articles", "Type");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Articles", new[] { "Type" });
            RenameColumn("dbo.Articles", "Type", "TypeNew");
            CreateTable(
                "dbo.ArticleTypes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 20),
                        Description = c.String(nullable: false, maxLength: 32),
                        AllowVote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Articles", "TypeId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.ArticleTypes", "Name");
            CreateIndex("dbo.Articles", "TypeId");
            AddForeignKey("dbo.Articles", "TypeId", "dbo.ArticleTypes", "Id");
        }
    }
}
