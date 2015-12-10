namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleTypeName : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ArticleTypes", "Name");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ArticleTypes", new[] { "Name" });
        }
    }
}
