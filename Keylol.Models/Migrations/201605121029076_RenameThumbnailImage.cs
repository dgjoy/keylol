namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameThumbnailImage : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Activities", "ThumbnailImage", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.Articles", "ThumbnailImage", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.ConferenceEntries", "ThumbnailImage", c => c.String(nullable: false, maxLength: 256));
            RenameColumn("dbo.Activities", "ThumbnailImage", "CoverImage");
            RenameColumn("dbo.Articles", "ThumbnailImage", "CoverImage");
            RenameColumn("dbo.ConferenceEntries", "ThumbnailImage", "CoverImage");
        }

        public override void Down()
        {
            RenameColumn("dbo.ConferenceEntries", "CoverImage", "ThumbnailImage");
            RenameColumn("dbo.Articles", "CoverImage", "ThumbnailImage");
            RenameColumn("dbo.Activities", "CoverImage", "ThumbnailImage");
            AlterColumn("dbo.ConferenceEntries", "ThumbnailImage", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Articles", "ThumbnailImage", c => c.String(nullable: false, maxLength: 1024));
            AlterColumn("dbo.Activities", "ThumbnailImage", c => c.String(nullable: false, maxLength: 1024));
        }
    }
}