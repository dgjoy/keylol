namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class DiscountedPrice : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Logs", "ArticleId", "dbo.Articles");
            DropForeignKey("dbo.Logs", "EditorId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Logs", "UserId", "dbo.KeylolUsers");

            RenameColumn("dbo.Points", "SteamDiscount", "SteamDiscountedPrice");
            RenameColumn("dbo.Points", "SonkwoDiscount", "SonkwoDiscountedPrice");
        }

        public override void Down()
        {
            RenameColumn("dbo.Points", "SonkwoDiscountedPrice", "SonkwoDiscount");
            RenameColumn("dbo.Points", "SteamDiscountedPrice", "SteamDiscount");

            AddForeignKey("dbo.Logs", "UserId", "dbo.KeylolUsers", "Id");
            AddForeignKey("dbo.Logs", "EditorId", "dbo.KeylolUsers", "Id");
            AddForeignKey("dbo.Logs", "ArticleId", "dbo.Articles", "Id");
        }
    }
}