namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EntryModelSimplify : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Entries", newName: "Articles");
            DropIndex("dbo.Articles", new[] { "TypeId" });
            DropIndex("dbo.Articles", new[] { "SequenceNumberForAuthor" });
            AlterColumn("dbo.Articles", "TypeId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Articles", "Title", c => c.String(nullable: false, maxLength: 120));
            AlterColumn("dbo.Articles", "Content", c => c.String(nullable: false));
            AlterColumn("dbo.Articles", "ThumbnailImage", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Articles", "UnstyledContent", c => c.String(nullable: false));
            AlterColumn("dbo.Articles", "SequenceNumberForAuthor", c => c.Int(nullable: false));
            AlterColumn("dbo.Articles", "IgnoreNewLikes", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Articles", "IgnoreNewComments", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Articles", "Pros", c => c.String(nullable: false));
            AlterColumn("dbo.Articles", "Cons", c => c.String(nullable: false));
            CreateIndex("dbo.Articles", "TypeId");
            CreateIndex("dbo.Articles", "SequenceNumberForAuthor");
            DropColumn("dbo.Articles", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Articles", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            DropIndex("dbo.Articles", new[] { "SequenceNumberForAuthor" });
            DropIndex("dbo.Articles", new[] { "TypeId" });
            AlterColumn("dbo.Articles", "Cons", c => c.String());
            AlterColumn("dbo.Articles", "Pros", c => c.String());
            AlterColumn("dbo.Articles", "IgnoreNewComments", c => c.Boolean());
            AlterColumn("dbo.Articles", "IgnoreNewLikes", c => c.Boolean());
            AlterColumn("dbo.Articles", "SequenceNumberForAuthor", c => c.Int());
            AlterColumn("dbo.Articles", "UnstyledContent", c => c.String());
            AlterColumn("dbo.Articles", "ThumbnailImage", c => c.String(maxLength: 1024));
            AlterColumn("dbo.Articles", "Content", c => c.String());
            AlterColumn("dbo.Articles", "Title", c => c.String(maxLength: 120));
            AlterColumn("dbo.Articles", "TypeId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Articles", "SequenceNumberForAuthor");
            CreateIndex("dbo.Articles", "TypeId");
            RenameTable(name: "dbo.Articles", newName: "Entries");
        }
    }
}
