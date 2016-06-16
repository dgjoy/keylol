namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ArticleReproductionRequirement : DbMigration
    {
        public override void Up()
        {
            Sql(@"DECLARE @pid nvarchar(256)
                SET @pid = (SELECT Top(1) Id From Points WHERE IdCode = 'STEAM')
                UPDATE Articles SET TargetPointId = @pid WHERE TargetPointId IS NULL");
            AddColumn("dbo.Articles", "ReproductionRequirement", c => c.String(nullable: false, maxLength: 1000));
        }

        public override void Down()
        {
            DropColumn("dbo.Articles", "ReproductionRequirement");
        }
    }
}