namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoreArchivedStates : DbMigration
    {
        public override void Up()
        {
            this.DeleteDefaultContraint("dbo.Articles", "Archived");
            this.DeleteDefaultContraint("dbo.Comments", "Archived");
            AlterColumn("dbo.Articles", "Archived", c => c.Int(nullable: false));
            AlterColumn("dbo.Comments", "Archived", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Comments", "Archived", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Articles", "Archived", c => c.Boolean(nullable: false));
        }
    }
}
