namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EntrySN : DbMigration
    {

        public override void Up()
        {
            Sql("CREATE SEQUENCE [dbo].[EntrySequence] AS int START WITH 1 INCREMENT BY 1");
            Sql("ALTER TABLE [dbo].[Entries] ADD [SequenceNumber] int NOT NULL DEFAULT NEXT VALUE FOR [dbo].[EntrySequence]");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Entries", "SequenceNumber");
            Sql("DROP SEQUENCE [dbo].[EntrySequence]");
        }
    }
}
