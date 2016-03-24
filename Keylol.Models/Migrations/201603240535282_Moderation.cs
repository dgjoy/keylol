namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Moderation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articles", "Archived", c => c.Boolean(nullable: false));
            AddColumn("dbo.Articles", "Rejected", c => c.Boolean(nullable: false));
            AddColumn("dbo.Articles", "Spotlight", c => c.Boolean(nullable: false));
            AddColumn("dbo.Articles", "Warned", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "Archived", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "Warned", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comments", "Warned");
            DropColumn("dbo.Comments", "Archived");
            DropColumn("dbo.Articles", "Warned");
            DropColumn("dbo.Articles", "Spotlight");
            DropColumn("dbo.Articles", "Rejected");
            DropColumn("dbo.Articles", "Archived");
        }
    }
}
