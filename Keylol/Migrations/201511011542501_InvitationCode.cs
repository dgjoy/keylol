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
                        Source = c.String(nullable: false, maxLength: 64),
                        UsedByUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KeylolUsers", t => t.UsedByUser_Id)
                .Index(t => t.GenerateTime)
                .Index(t => t.Source)
                .Index(t => t.UsedByUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InvitationCodes", "UsedByUser_Id", "dbo.KeylolUsers");
            DropIndex("dbo.InvitationCodes", new[] { "UsedByUser_Id" });
            DropIndex("dbo.InvitationCodes", new[] { "Source" });
            DropIndex("dbo.InvitationCodes", new[] { "GenerateTime" });
            DropTable("dbo.InvitationCodes");
        }
    }
}
