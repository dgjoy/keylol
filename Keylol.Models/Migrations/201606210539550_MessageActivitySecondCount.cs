namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MessageActivitySecondCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "ActivityId", c => c.String(maxLength: 128));
            AddColumn("dbo.Messages", "SecondCount", c => c.Int(nullable: false));
            CreateIndex("dbo.Messages", "ActivityId");
            AddForeignKey("dbo.Messages", "ActivityId", "dbo.Activities", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Messages", "ActivityId", "dbo.Activities");
            DropIndex("dbo.Messages", new[] { "ActivityId" });
            DropColumn("dbo.Messages", "SecondCount");
            DropColumn("dbo.Messages", "ActivityId");
        }
    }
}
