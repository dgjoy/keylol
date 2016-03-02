namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUselessSubscriptions : DbMigration
    {
        public override void Up()
        {
            Sql(@"DELETE FROM [dbo].[UserPointSubscriptions] WHERE NOT EXISTS (
                    (SELECT 1 FROM [dbo].[NormalPoints] WHERE [dbo].[NormalPoints].[Id] = [dbo].[UserPointSubscriptions].[Point_Id])
                    UNION
                    (SELECT 1 FROM [dbo].[KeylolUsers] WHERE [dbo].[KeylolUsers].[Id] = [dbo].[UserPointSubscriptions].[Point_Id])
                )");
        }
        
        public override void Down()
        {
        }
    }
}
