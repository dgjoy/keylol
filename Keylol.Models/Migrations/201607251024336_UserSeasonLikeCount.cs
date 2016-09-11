namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserSeasonLikeCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "SeasonLikeCount", c => c.Int(nullable: false));
            CreateIndex("dbo.KeylolUsers", "SeasonLikeCount");
        }
        
        public override void Down()
        {
            DropIndex("dbo.KeylolUsers", new[] { "SeasonLikeCount" });
            DropColumn("dbo.KeylolUsers", "SeasonLikeCount");
        }
    }
}
