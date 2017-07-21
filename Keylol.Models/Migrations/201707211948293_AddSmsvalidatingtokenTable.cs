namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSmsvalidatingtokenTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SmsValidatingTokens",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PhoneNumber = c.String(nullable: false, maxLength: 11),
                        Code = c.String(maxLength: 4),
                        SentTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.PhoneNumber, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SmsValidatingTokens", new[] { "PhoneNumber" });
            DropTable("dbo.SmsValidatingTokens");
        }
    }
}
