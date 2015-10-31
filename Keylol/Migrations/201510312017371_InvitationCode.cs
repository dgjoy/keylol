namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvitationCode : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvitationCodes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        GenerateTime = c.DateTime(nullable: false),
                        UserByUserId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.GenerateTime);
            
            AddColumn("dbo.KeylolUsers", "InvitationCodeId", c => c.String());
            AddColumn("dbo.KeylolUsers", "InvitationCode_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.KeylolUsers", "InvitationCode_Id");
            AddForeignKey("dbo.KeylolUsers", "InvitationCode_Id", "dbo.InvitationCodes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.KeylolUsers", "InvitationCode_Id", "dbo.InvitationCodes");
            DropIndex("dbo.InvitationCodes", new[] { "GenerateTime" });
            DropIndex("dbo.KeylolUsers", new[] { "InvitationCode_Id" });
            DropColumn("dbo.KeylolUsers", "InvitationCode_Id");
            DropColumn("dbo.KeylolUsers", "InvitationCodeId");
            DropTable("dbo.InvitationCodes");
        }
    }
}
