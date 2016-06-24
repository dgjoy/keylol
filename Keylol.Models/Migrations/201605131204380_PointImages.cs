namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointImages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Points", "TitleCoverImage", c => c.String(nullable: false, maxLength: 256));
            AddColumn("dbo.Points", "ThumbnailImage", c => c.String(nullable: false, maxLength: 256));
            DropColumn("dbo.Points", "InboxHeaderImage");
            DropColumn("dbo.Points", "CapsuleImage");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Points", "CapsuleImage", c => c.String(nullable: false, maxLength: 256));
            AddColumn("dbo.Points", "InboxHeaderImage", c => c.String(nullable: false, maxLength: 256));
            DropColumn("dbo.Points", "ThumbnailImage");
            DropColumn("dbo.Points", "TitleCoverImage");
        }
    }
}
