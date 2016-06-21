namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArticleCommentUnstyledContent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeylolUsers", "NotifyOnActivityReplied", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "NotifyOnActivityLiked", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnActivityReplied", c => c.Boolean(nullable: false));
            AddColumn("dbo.KeylolUsers", "SteamNotifyOnActivityLiked", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArticleComments", "UnstyledContent", c => c.String(nullable: false));
            Sql(@"UPDATE ArticleComments SET UnstyledContent = Content");
        }
        
        public override void Down()
        {
            DropColumn("dbo.ArticleComments", "UnstyledContent");
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnActivityLiked");
            DropColumn("dbo.KeylolUsers", "SteamNotifyOnActivityReplied");
            DropColumn("dbo.KeylolUsers", "NotifyOnActivityLiked");
            DropColumn("dbo.KeylolUsers", "NotifyOnActivityReplied");
        }
    }
}
