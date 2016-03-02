namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SequenceNoCache : DbMigration
    {
        public override void Up()
        {
            // 此处写入 Top(100000) 假定目前数量不超过十万
            Sql(@"ALTER SEQUENCE [dbo].[EntrySequence] RESTART NO CACHE");
            Sql(@"WITH cte AS (SELECT TOP(100000) * FROM [dbo].[Entries] ORDER BY [SequenceNumber])
                UPDATE cte SET [SequenceNumber] = NEXT VALUE FOR [dbo].[EntrySequence]");
            Sql(@"ALTER SEQUENCE [dbo].[UserSequence] RESTART NO CACHE");
            Sql(@"WITH cte AS (SELECT TOP(100000) * FROM [dbo].[KeylolUsers] ORDER BY [SequenceNumber])
                UPDATE cte SET [SequenceNumber] = NEXT VALUE FOR [dbo].[UserSequence]");
        }
        
        public override void Down()
        {
            Sql(@"ALTER SEQUENCE [dbo].[UserSequence] CACHE");
            Sql(@"ALTER SEQUENCE [dbo].[EntrySequence] CACHE");
        }
    }
}
