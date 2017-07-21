namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifySmsValidatingTokenTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmsValidatingTokens", "PhoneNumberConfirmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SmsValidatingTokens", "PhoneNumberConfirmed");
        }
    }
}
