namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserSN : DbMigration
    {
        public override void Up()
        {
            Sql("CREATE SEQUENCE [dbo].[UserSequence] AS int START WITH 1 INCREMENT BY 1");
            Sql("ALTER TABLE [dbo].[KeylolUsers] ADD [SequenceNumber] int NOT NULL DEFAULT NEXT VALUE FOR [dbo].[UserSequence]");
            CreateIndex("dbo.KeylolUsers", "SequenceNumber");
        }

        public override void Down()
        {
            DropIndex("dbo.KeylolUsers", new[] { "SequenceNumber" });
            DropColumn("dbo.KeylolUsers", "SequenceNumber");
            Sql("DROP SEQUENCE [dbo].[UserSequence]");
        }
    }
}
