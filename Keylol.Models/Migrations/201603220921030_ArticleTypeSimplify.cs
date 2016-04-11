namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleTypeSimplify : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Articles", "TypeNew", c => c.Int(nullable: false));
            Sql(@"UPDATE [t1]
                    SET [t1].[TypeNew] = CASE [t2].[Name]
                        WHEN 'ºÚ∆¿' THEN 0
                        WHEN '∆¿' THEN 1
                        WHEN '—–' THEN 2
                        WHEN 'Ã∏' THEN 3
                        WHEN '—∂' THEN 4
                        ELSE 5
                    END
                    FROM [dbo].[Articles] AS [t1]
                    LEFT JOIN [dbo].[ArticleTypes] AS [t2] on [t1].[TypeId] = [t2].[Id]");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Articles", "TypeNew");
        }
    }
}
