namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserInvitor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "InviterId", c => c.String(maxLength: 128));
            CreateIndex("dbo.KeylolUsers", "InviterId");
            AddForeignKey("dbo.KeylolUsers", "InviterId", "dbo.KeylolUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.KeylolUsers", "InviterId", "dbo.KeylolUsers");
            DropIndex("dbo.KeylolUsers", new[] { "InviterId" });
            DropColumn("dbo.KeylolUsers", "InviterId");
        }
    }
}
