namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class MessageSequenceNumber : DbMigration
    {
        public override void Up()
        {
            Sql("CREATE SEQUENCE [dbo].[MessageSequence] AS int START WITH 1 INCREMENT BY 1 NO CACHE");
            Sql(
                "ALTER TABLE [dbo].[Messages] ADD [SequenceNumber] int NOT NULL DEFAULT NEXT VALUE FOR [dbo].[MessageSequence]");
            CreateIndex("dbo.Messages", "SequenceNumber", unique: true);
            this.DeleteDefaultContraint("dbo.Articles", "SequenceNumber");
            Sql("EXECUTE sp_rename N'EntrySequence', N'ArticleSequence'");
            Sql(
                "ALTER TABLE [dbo].[Articles] ADD DEFAULT NEXT VALUE FOR [dbo].[MessageSequence] FOR [SequenceNumber]");
        }

        public override void Down()
        {
            this.DeleteDefaultContraint("dbo.Articles", "SequenceNumber");
            Sql("EXECUTE sp_rename N'ArticleSequence', N'EntrySequence'");
            Sql(
                "ALTER TABLE [dbo].[Articles] ADD DEFAULT NEXT VALUE FOR [dbo].[EntrySequence] FOR [SequenceNumber]");
            DropIndex("dbo.Messages", new[] {"SequenceNumber"});
            DropColumn("dbo.Messages", "SequenceNumber");
            Sql("DROP SEQUENCE [dbo].[MessageSequence]");
        }
    }
}