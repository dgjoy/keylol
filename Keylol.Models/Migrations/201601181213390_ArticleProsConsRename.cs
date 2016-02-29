namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleProsConsRename : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Entries", "Goodness", "Pros");
            RenameColumn("dbo.Entries", "Badness", "Cons");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Entries", "Cons", "Badness");
            RenameColumn("dbo.Entries", "Pros", "Goodness");
        }
    }
}
