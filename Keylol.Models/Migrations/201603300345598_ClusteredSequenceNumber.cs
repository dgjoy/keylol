namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClusteredSequenceNumber : DbMigration
    {
        public override void Up()
        {
            Sql("CREATE SEQUENCE [dbo].[CommentSequence] AS int START WITH 1 INCREMENT BY 1 NO CACHE");
            AddColumn("dbo.Comments", "SequenceNumber", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[CommentSequence]"));
            CreateIndex("dbo.Comments", "SequenceNumber", unique: true);

            // 此处写入 Top(100000) 假定目前数量不超过十万
            Sql(@"ALTER SEQUENCE [dbo].[CommentSequence] RESTART NO CACHE");
            Sql(@"WITH cte AS (SELECT TOP(100000) * FROM [dbo].[Comments] ORDER BY [PublishTime])
                UPDATE cte SET [SequenceNumber] = NEXT VALUE FOR [dbo].[CommentSequence]");

            // 因为调整 Clustered Index 过于复杂，此处需要人工手动在 SSMS 中调整以下表的 Clustered Index 至 SequenceNumber 列
            // KeylolUser
            // Articles
            // Comments
            // Messages
            // 调整之后 Sequence 计数器可能发生变化，需要注意纠正
            // 参照以下 SQL
            /*
                ALTER SEQUENCE [dbo].[UserSequence] RESTART NO CACHE;
                WITH cte AS (SELECT TOP(100000) * FROM [dbo].[KeylolUsers] ORDER BY [SequenceNumber])
                UPDATE cte SET [SequenceNumber] = NEXT VALUE FOR [dbo].[UserSequence];

                ALTER SEQUENCE [dbo].[MessageSequence] RESTART NO CACHE;
                WITH cte AS (SELECT TOP(100000) * FROM [dbo].[Messages] ORDER BY [SequenceNumber])
                UPDATE cte SET [SequenceNumber] = NEXT VALUE FOR [dbo].[MessageSequence];

                ALTER SEQUENCE [dbo].[ArticleSequence] RESTART NO CACHE;
                WITH cte AS (SELECT TOP(100000) * FROM [dbo].[Articles] ORDER BY [SequenceNumber])
                UPDATE cte SET [SequenceNumber] = NEXT VALUE FOR [dbo].[ArticleSequence];

                ALTER SEQUENCE [dbo].[CommentSequence] RESTART NO CACHE;
                WITH cte AS (SELECT TOP(100000) * FROM [dbo].[Comments] ORDER BY [PublishTime])
                UPDATE cte SET [SequenceNumber] = NEXT VALUE FOR [dbo].[CommentSequence];
            */

        }

        public override void Down()
        {
            DropIndex("dbo.Comments", new[] { "SequenceNumber" });
            DropColumn("dbo.Comments", "SequenceNumber");
            Sql("DROP SEQUENCE [dbo].[CommentSequence]");
        }
    }
}
