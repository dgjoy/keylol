namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointsFullTextIndex : DbMigration
    {
        public override void Up()
        {
            Sql(@"CREATE FULLTEXT CATALOG [default] WITH ACCENT_SENSITIVITY = OFF AS DEFAULT", true);
            Sql(@"CREATE FULLTEXT INDEX ON [dbo].[NormalPoints] (
	                [ChineseName] LANGUAGE 2052,
	                [ChineseAliases] LANGUAGE 2052,
	                [EnglishName] LANGUAGE 1033,
	                [EnglishAliases] LANGUAGE 1033
                ) KEY INDEX [PK_dbo.NormalPoints]", true);
        }
        
        public override void Down()
        {
            Sql(@"DROP FULLTEXT INDEX ON [dbo].[NormalPoints]", true);
            Sql(@"DROP FULLTEXT CATALOG [default]", true);
        }
    }
}
