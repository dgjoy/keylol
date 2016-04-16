namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserGameRecordIdentity : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.UserGameRecords");
            DropColumn("dbo.UserGameRecords", "Id");
            AddColumn("dbo.UserGameRecords", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.UserGameRecords", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.UserGameRecords");
            AlterColumn("dbo.UserGameRecords", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.UserGameRecords", "Id");
        }
    }
}
