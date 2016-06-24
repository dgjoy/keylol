namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LongId : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Activities", new[] { "Sid" });
            DropIndex("dbo.SteamStoreNames", new[] { "Sid" });
            DropIndex("dbo.ActivityComments", new[] { "Sid" });
            DropIndex("dbo.ArticleComments", new[] { "Sid" });
            DropIndex("dbo.Articles", new[] { "Sid" });
            DropIndex("dbo.AtRecords", new[] { "Sid" });
            DropIndex("dbo.CouponLogs", new[] { "Sid" });
            DropIndex("dbo.Likes", new[] { "Sid" });
            DropIndex("dbo.Messages", new[] { "Sid" });
            DropIndex("dbo.Replies", new[] { "Sid" });
            DropIndex("dbo.Subscriptions", new[] { "Sid" });
            DropPrimaryKey("dbo.Feeds");
            DropPrimaryKey("dbo.UserSteamFriendRecords");
            DropPrimaryKey("dbo.UserSteamGameRecords");
            this.DeleteDefaultContraint("dbo.Activities", "Sid");
            this.DeleteDefaultContraint("dbo.SteamStoreNames", "Sid");
            this.DeleteDefaultContraint("dbo.ActivityComments", "Sid");
            this.DeleteDefaultContraint("dbo.ArticleComments", "Sid");
            this.DeleteDefaultContraint("dbo.Articles", "Sid");
            this.DeleteDefaultContraint("dbo.AtRecords", "Sid");
            this.DeleteDefaultContraint("dbo.CouponLogs", "Sid");
            this.DeleteDefaultContraint("dbo.Likes", "Sid");
            this.DeleteDefaultContraint("dbo.Messages", "Sid");
            this.DeleteDefaultContraint("dbo.Replies", "Sid");
            this.DeleteDefaultContraint("dbo.Subscriptions", "Sid");
            AlterColumn("dbo.Activities", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ActivitySid]"));
            AlterColumn("dbo.SteamStoreNames", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SteamStoreNameSid]"));
            AlterColumn("dbo.ActivityComments", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ActivityCommentSid]"));
            AlterColumn("dbo.ArticleComments", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ArticleCommentSid]"));
            AlterColumn("dbo.Articles", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ArticleSid]"));
            AlterColumn("dbo.AtRecords", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[AtRecordSid]"));
            AlterColumn("dbo.CouponLogs", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[CouponLogSid]"));
            AlterColumn("dbo.Feeds", "Id", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.Likes", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[LikeSid]"));
            AlterColumn("dbo.Messages", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[MessageSid]"));
            AlterColumn("dbo.Replies", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ReplySid]"));
            AlterColumn("dbo.Subscriptions", "Sid", c => c.Long(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SubscriptionSid]"));
            AlterColumn("dbo.UserSteamFriendRecords", "Id", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.UserSteamGameRecords", "Id", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.Feeds", "Id");
            AddPrimaryKey("dbo.UserSteamFriendRecords", "Id");
            AddPrimaryKey("dbo.UserSteamGameRecords", "Id");
            CreateIndex("dbo.Activities", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.SteamStoreNames", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.ActivityComments", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.ArticleComments", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Articles", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.AtRecords", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.CouponLogs", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Likes", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Messages", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Replies", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Subscriptions", "Sid", unique: true, clustered: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Subscriptions", new[] { "Sid" });
            DropIndex("dbo.Replies", new[] { "Sid" });
            DropIndex("dbo.Messages", new[] { "Sid" });
            DropIndex("dbo.Likes", new[] { "Sid" });
            DropIndex("dbo.CouponLogs", new[] { "Sid" });
            DropIndex("dbo.AtRecords", new[] { "Sid" });
            DropIndex("dbo.Articles", new[] { "Sid" });
            DropIndex("dbo.ArticleComments", new[] { "Sid" });
            DropIndex("dbo.ActivityComments", new[] { "Sid" });
            DropIndex("dbo.SteamStoreNames", new[] { "Sid" });
            DropIndex("dbo.Activities", new[] { "Sid" });
            DropPrimaryKey("dbo.UserSteamGameRecords");
            DropPrimaryKey("dbo.UserSteamFriendRecords");
            DropPrimaryKey("dbo.Feeds");
            this.DeleteDefaultContraint("dbo.Activities", "Sid");
            this.DeleteDefaultContraint("dbo.SteamStoreNames", "Sid");
            this.DeleteDefaultContraint("dbo.ActivityComments", "Sid");
            this.DeleteDefaultContraint("dbo.ArticleComments", "Sid");
            this.DeleteDefaultContraint("dbo.Articles", "Sid");
            this.DeleteDefaultContraint("dbo.AtRecords", "Sid");
            this.DeleteDefaultContraint("dbo.CouponLogs", "Sid");
            this.DeleteDefaultContraint("dbo.Likes", "Sid");
            this.DeleteDefaultContraint("dbo.Messages", "Sid");
            this.DeleteDefaultContraint("dbo.Replies", "Sid");
            this.DeleteDefaultContraint("dbo.Subscriptions", "Sid");
            AlterColumn("dbo.Activities", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ActivitySid]"));
            AlterColumn("dbo.SteamStoreNames", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SteamStoreNameSid]"));
            AlterColumn("dbo.ActivityComments", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ActivityCommentSid]"));
            AlterColumn("dbo.ArticleComments", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ArticleCommentSid]"));
            AlterColumn("dbo.Articles", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ArticleSid]"));
            AlterColumn("dbo.AtRecords", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[AtRecordSid]"));
            AlterColumn("dbo.CouponLogs", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[CouponLogSid]"));
            AlterColumn("dbo.Feeds", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Likes", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[LikeSid]"));
            AlterColumn("dbo.Messages", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[MessageSid]"));
            AlterColumn("dbo.Replies", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ReplySid]"));
            AlterColumn("dbo.Subscriptions", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SubscriptionSid]"));
            AlterColumn("dbo.UserSteamFriendRecords", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.UserSteamGameRecords", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.UserSteamGameRecords", "Id");
            AddPrimaryKey("dbo.UserSteamFriendRecords", "Id");
            AddPrimaryKey("dbo.Feeds", "Id");
            CreateIndex("dbo.Subscriptions", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Replies", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Messages", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Likes", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.CouponLogs", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.AtRecords", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Articles", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.ArticleComments", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.ActivityComments", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.SteamStoreNames", "Sid", unique: true, clustered: true);
            CreateIndex("dbo.Activities", "Sid", unique: true, clustered: true);
        }
    }
}
