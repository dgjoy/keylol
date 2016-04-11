namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserUniqueSequenceNumber : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.KeylolUsers", new[] { "SequenceNumber" });
            CreateIndex("dbo.KeylolUsers", "SequenceNumber", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.KeylolUsers", new[] { "SequenceNumber" });
            CreateIndex("dbo.KeylolUsers", "SequenceNumber");
        }
    }
}
