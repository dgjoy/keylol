namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FullIndexRecreate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articles", "UnstyledContent", c => c.String(nullable: false));
            Sql(@"DROP FULLTEXT INDEX ON [dbo].[Points]", true);
            Sql(@"DROP FULLTEXT INDEX ON [dbo].[Articles]", true);
            Sql(@"CREATE FULLTEXT INDEX ON [dbo].[Points] (
	                [ChineseName] LANGUAGE 2052,
	                [ChineseAliases] LANGUAGE 2052,
	                [EnglishName] LANGUAGE 1033,
	                [EnglishAliases] LANGUAGE 1033
                ) KEY INDEX [IX_Sid]", true);
            Sql(@"CREATE FULLTEXT INDEX ON [dbo].[Articles] (
	                [Title] LANGUAGE 2052,
                    [Subtitle] LANGUAGE 2052,
	                [UnstyledContent] LANGUAGE 2052
                ) KEY INDEX [IX_Sid]", true);
        }
        
        public override void Down()
        {
            Sql(@"DROP FULLTEXT INDEX ON [dbo].[Articles]", true);
            Sql(@"DROP FULLTEXT INDEX ON [dbo].[Points]", true);
            Sql(@"CREATE FULLTEXT INDEX ON [dbo].[Articles] (
	                [Title] LANGUAGE 2052,
                    [Subtitle] LANGUAGE 2052,
	                [Content] LANGUAGE 2052
                ) KEY INDEX [PX_dbo.Articles]", true);
            Sql(@"CREATE FULLTEXT INDEX ON [dbo].[Points] (
	                [ChineseName] LANGUAGE 2052,
	                [ChineseAliases] LANGUAGE 2052,
	                [EnglishName] LANGUAGE 1033,
	                [EnglishAliases] LANGUAGE 1033
                ) KEY INDEX [PK_dbo.Points]", true);
            DropColumn("dbo.Articles", "UnstyledContent");
        }
    }
}
