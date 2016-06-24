namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Feed : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Feeds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StreamName = c.String(nullable: false, maxLength: 128),
                        Time = c.DateTime(nullable: false),
                        EntryType = c.Int(nullable: false),
                        Entry = c.String(nullable: false, maxLength: 400),
                        Properties = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.StreamName)
                .Index(t => t.Entry);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Feeds", new[] { "Entry" });
            DropIndex("dbo.Feeds", new[] { "StreamName" });
            DropTable("dbo.Feeds");
        }
    }
}
