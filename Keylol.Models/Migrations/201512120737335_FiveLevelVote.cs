namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class FiveLevelVote : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE [dbo].[Entries] SET [Vote] = 4 WHERE [Vote] = 0");
            Sql(@"UPDATE [dbo].[Entries] SET [Vote] = 2 WHERE [Vote] = 1");
            Sql(@"UPDATE [dbo].[Entries] SET [Vote] = 3 WHERE [Vote] IS NULL AND [VoteForPointId] IS NOT NULL");
        }

        public override void Down()
        {
            Sql(@"UPDATE [dbo].[Entries] SET [Vote] = 1 WHERE [Vote] <= 2");
            Sql(@"UPDATE [dbo].[Entries] SET [Vote] = 0 WHERE [Vote] >= 4");
            Sql(@"UPDATE [dbo].[Entries] SET [Vote] = NULL WHERE [Vote] = 3");
        }
    }
}