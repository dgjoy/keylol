namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleRowVersion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Entries", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Entries", "RowVersion");
        }
    }
}
