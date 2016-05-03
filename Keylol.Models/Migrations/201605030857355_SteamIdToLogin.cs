namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SteamIdToLogin : DbMigration
    {
        public override void Up()
        {
            Sql(@"WITH [Login](UserId, ProviderKey, LoginProvider) AS (
                    SELECT Id AS UserId, SteamId AS ProviderKey, 'Steam' AS LoginProvider FROM [dbo].[KeylolUsers]
                )
                INSERT INTO [dbo].[UserLogins] (UserId, ProviderKey, LoginProvider)
                SELECT * FROM [Login]");
            DropIndex("dbo.KeylolUsers", new[] { "SteamId" });
            DropColumn("dbo.KeylolUsers", "SteamId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KeylolUsers", "SteamId", c => c.String(maxLength: 64));
            CreateIndex("dbo.KeylolUsers", "SteamId");
        }
    }
}
