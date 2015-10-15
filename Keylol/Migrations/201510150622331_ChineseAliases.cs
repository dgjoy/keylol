namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChineseAliases : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.NormalPoints", "Aliases", "EnglishAliases");
            AddColumn("dbo.NormalPoints", "ChineseAliases", c => c.String(nullable: false, maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NormalPoints", "ChineseAliases");
            RenameColumn("dbo.NormalPoints", "EnglishAliases", "Aliases");
        }
    }
}
