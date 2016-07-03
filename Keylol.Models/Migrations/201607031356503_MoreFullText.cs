namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoreFullText : DbMigration
    {
        public override void Up()
        {
            Sql(@"ALTER FULLTEXT INDEX ON [dbo].[Articles] ADD ([Subtitle] LANGUAGE 2052)",true);
            Sql(@"CREATE FULLTEXT INDEX ON [dbo].[KeylolUsers] ([UserName] LANGUAGE 2052) KEY INDEX [IX_Sid]", true);
            //Sql(@"ALTER FULLTEXT INDEX ON [dbo].[KeylolUsers] ENABLE");
        }
        
        public override void Down()
        {
            Sql(@"DROP FULLTEXT INDEX ON [dbo].[KeylolUsers]",true);
            Sql(@"ALTER FULLTEXT INDEX ON [dbo].[Articles] DROP ([Subtitle])", true);
        }
    }
}
