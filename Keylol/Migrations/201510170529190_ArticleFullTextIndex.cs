namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleFullTextIndex : DbMigration
    {
        public override void Up()
        {
            Sql(@"CREATE FULLTEXT INDEX ON [dbo].[Entries] (
	                [Title] LANGUAGE 2052,
	                [Content] LANGUAGE 2052
                ) KEY INDEX [PK_dbo.Entries]", true);
        }
        
        public override void Down()
        {
            Sql(@"DROP FULLTEXT INDEX ON [dbo].[Entries]", true);
        }
    }
}
