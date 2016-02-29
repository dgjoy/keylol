namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleGoodnessBadness : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Entries", "Goodness", c => c.String());
            AddColumn("dbo.Entries", "Badness", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Entries", "Badness");
            DropColumn("dbo.Entries", "Goodness");
        }
    }
}
