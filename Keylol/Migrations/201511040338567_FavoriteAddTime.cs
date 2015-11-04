namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FavoriteAddTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Favorites", "AddTime", c => c.DateTime(nullable: false));
            CreateIndex("dbo.Favorites", "AddTime");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Favorites", new[] { "AddTime" });
            DropColumn("dbo.Favorites", "AddTime");
        }
    }
}
