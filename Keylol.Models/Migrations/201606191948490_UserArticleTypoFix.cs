namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UserArticleTypoFix : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.KeylolUsers", "NotifyOnArtivleReplied", "NotifyOnArticleReplied");
            RenameColumn("dbo.KeylolUsers", "NotifyOnArtivleLiked", "NotifyOnArticleLiked");
        }

        public override void Down()
        {
            RenameColumn("dbo.KeylolUsers", "NotifyOnArticleLiked", "NotifyOnArtivleLiked");
            RenameColumn("dbo.KeylolUsers", "NotifyOnArticleReplied", "NotifyOnArtivleReplied");
        }
    }
}