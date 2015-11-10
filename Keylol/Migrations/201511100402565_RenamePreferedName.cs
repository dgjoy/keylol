namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamePreferedName : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.NormalPoints", "PreferedName", "PreferredName");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.NormalPoints", "PreferredName", "PreferedName");
        }
    }
}
