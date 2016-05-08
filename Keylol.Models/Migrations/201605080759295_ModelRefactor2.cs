namespace Keylol.Models.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ModelRefactor2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CommentReplies", "CommentId", "dbo.ArticleComments");
            DropForeignKey("dbo.CommentReplies", "ReplyId", "dbo.ArticleComments");
            RenameTable("dbo.CommentReplies", "Replies");

            RenameColumn("dbo.Articles", "SequenceNumber", "Sid");
            RenameIndex("dbo.Articles", "IX_SequenceNumber", "IX_Sid");
            this.DeleteDefaultContraint("dbo.Articles", "Sid");
            this.RenameSequence("dbo.ArticleSequence", "ArticleSid");
            this.AddDefaultContraint("dbo.Articles", "Sid", "NEXT VALUE FOR [dbo].[ArticleSid]");

            RenameColumn("dbo.Articles", "SequenceNumberForAuthor", "SidForAuthor");
            RenameIndex("dbo.Articles", "IX_SequenceNumberForAuthor", "IX_SidForAuthor");

            RenameColumn("dbo.KeylolUsers", "SequenceNumber", "Sid");
            RenameIndex("dbo.KeylolUsers", "IX_SequenceNumber", "IX_Sid");
            this.DeleteDefaultContraint("dbo.KeylolUsers", "Sid");
            this.RenameSequence("dbo.UserSequence", "UserSid");
            this.AddDefaultContraint("dbo.KeylolUsers", "Sid", "NEXT VALUE FOR [dbo].[UserSid]");

            RenameColumn("dbo.ArticleComments", "SequenceNumber", "Sid");
            RenameIndex("dbo.ArticleComments", "IX_SequenceNumber", "IX_Sid");
            this.DeleteDefaultContraint("dbo.ArticleComments", "Sid");
            this.RenameSequence("dbo.CommentSequence", "ArticleCommentSid");
            this.AddDefaultContraint("dbo.ArticleComments", "Sid", "NEXT VALUE FOR [dbo].[ArticleCommentSid]");

            RenameColumn("dbo.ArticleComments", "SequenceNumberForArticle", "SidForArticle");
            RenameIndex("dbo.ArticleComments", "IX_SequenceNumberForArticle", "IX_SidForArticle");

            RenameColumn("dbo.SteamBots", "SequenceNumber", "Sid");
            RenameIndex("dbo.SteamBots", "IX_SequenceNumber", "IX_Sid");
            this.DeleteDefaultContraint("dbo.SteamBots", "Sid");
            this.RenameSequence("dbo.SteamBotSequence", "SteamBotSid");
            this.AddDefaultContraint("dbo.SteamBots", "Sid", "NEXT VALUE FOR [dbo].[SteamBotSid]");

            RenameColumn("dbo.CouponLogs", "SequenceNumber", "Sid");
            RenameIndex("dbo.CouponLogs", "IX_SequenceNumber", "IX_Sid");
            this.DeleteDefaultContraint("dbo.CouponLogs", "Sid");
            this.RenameSequence("dbo.CouponLogSequence", "CouponLogSid");
            this.AddDefaultContraint("dbo.CouponLogs", "Sid", "NEXT VALUE FOR [dbo].[CouponLogSid]");

            RenameColumn("dbo.Messages", "SequenceNumber", "Sid");
            RenameIndex("dbo.Messages", "IX_SequenceNumber", "IX_Sid");
            this.DeleteDefaultContraint("dbo.Messages", "Sid");
            this.RenameSequence("dbo.MessageSequence", "MessageSid");
            this.AddDefaultContraint("dbo.Messages", "Sid", "NEXT VALUE FOR [dbo].[MessageSid]");

            RenameColumn("dbo.Replies", "CommentId", "EntryId");
            RenameIndex("dbo.Replies", "IX_CommentId", "IX_EntryId");

            AddColumn("dbo.Replies", "EntryType", c => c.Int(false));

            this.CreateSequence("ActivitySid");
            this.CreateSequence("ActivityCommentSid");
            this.CreateSequence("AtRecordSid");
            this.CreateSequence("ConferenceEntrySid");
            this.CreateSequence("ConferenceSid");

            CreateTable(
                "dbo.Activities",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Sid = c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ActivitySid]"),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    PublishTime = c.DateTime(nullable: false),
                    AuthorId = c.String(nullable: false, maxLength: 128),
                    SidForAuthor = c.Int(nullable: false),
                    Content = c.String(nullable: false),
                    ThumbnailImage = c.String(nullable: false, maxLength: 1024),
                    DismissLikeMessage = c.Boolean(nullable: false),
                    DismissCommentMessage = c.Boolean(nullable: false),
                    TargetPointId = c.String(maxLength: 128),
                    Rating = c.Int(),
                    AttachedPoints = c.String(nullable: false),
                    Deleted = c.Int(nullable: false),
                    Archived = c.Int(nullable: false),
                    Rejected = c.Boolean(nullable: false),
                    Warned = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.KeylolUsers", t => t.AuthorId)
                .ForeignKey("dbo.NormalPoints", t => t.TargetPointId)
                .Index(t => t.Sid, unique: true, clustered: true)
                .Index(t => t.PublishTime)
                .Index(t => t.AuthorId)
                .Index(t => t.SidForAuthor)
                .Index(t => t.TargetPointId);

            CreateTable(
                "dbo.ActivityComments",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Sid = c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ActivityCommentSid]"),
                    Content = c.String(nullable: false),
                    PublishTime = c.DateTime(nullable: false),
                    CommentatorId = c.String(nullable: false, maxLength: 128),
                    ActivityId = c.String(nullable: false, maxLength: 128),
                    SidForActivity = c.Int(nullable: false),
                    DismissLikeMessage = c.Boolean(nullable: false),
                    DismissReplyMessage = c.Boolean(nullable: false),
                    Deleted = c.Int(nullable: false),
                    Warned = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.Activities", t => t.ActivityId)
                .ForeignKey("dbo.KeylolUsers", t => t.CommentatorId)
                .Index(t => t.Sid, unique: true, clustered: true)
                .Index(t => t.PublishTime)
                .Index(t => t.CommentatorId)
                .Index(t => t.ActivityId)
                .Index(t => t.SidForActivity);

            CreateTable(
                "dbo.AtRecords",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Sid = c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[AtRecordSid]"),
                    EntryType = c.Int(nullable: false),
                    EntryId = c.String(nullable: false),
                    UserId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.Sid, unique: true, clustered: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.ConferenceEntries",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Sid = c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ConferenceEntrySid]"),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    PublishTime = c.DateTime(nullable: false),
                    AuthorId = c.String(nullable: false, maxLength: 128),
                    SidForAuthor = c.Int(nullable: false),
                    ConferenceId = c.String(nullable: false, maxLength: 128),
                    SidForConference = c.Int(nullable: false),
                    Title = c.String(nullable: false, maxLength: 120),
                    Content = c.String(nullable: false),
                    ThumbnailImage = c.String(nullable: false, maxLength: 1024),
                })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.KeylolUsers", t => t.AuthorId)
                .ForeignKey("dbo.Conferences", t => t.ConferenceId)
                .Index(t => t.Sid, unique: true, clustered: true)
                .Index(t => t.PublishTime)
                .Index(t => t.AuthorId)
                .Index(t => t.SidForAuthor)
                .Index(t => t.ConferenceId)
                .Index(t => t.SidForConference);

            CreateTable(
                "dbo.Conferences",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Sid = c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ConferenceSid]"),
                })
                .PrimaryKey(t => t.Id, clustered: false)
                .Index(t => t.Sid, unique: true, clustered: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ConferenceEntries", "AuthorId", "dbo.KeylolUsers");
            DropForeignKey("dbo.ConferenceEntries", "ConferenceId", "dbo.Conferences");
            DropForeignKey("dbo.AtRecords", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.ActivityComments", "ActivityId", "dbo.Activities");
            DropForeignKey("dbo.ActivityComments", "CommentatorId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Activities", "AuthorId", "dbo.KeylolUsers");
            DropForeignKey("dbo.Activities", "TargetPointId", "dbo.NormalPoints");

            DropTable("dbo.Conferences");
            DropTable("dbo.ConferenceEntries");
            DropTable("dbo.AtRecords");
            DropTable("dbo.ActivityComments");
            DropTable("dbo.Activities");

            this.DropSequence("ConferenceSid");
            this.DropSequence("ConferenceEntrySid");
            this.DropSequence("AtRecordSid");
            this.DropSequence("ActivityCommentSid");
            this.DropSequence("ActivitySid");

            DropColumn("dbo.Replies", "EntryType");

            RenameColumn("dbo.Replies", "EntryId", "CommentId");
            RenameIndex("dbo.Replies", "IX_EntryId", "IX_CommentId");

            RenameColumn("dbo.Messages", "Sid", "SequenceNumber");
            RenameIndex("dbo.Messages", "IX_Sid", "IX_SequenceNumber");
            this.DeleteDefaultContraint("dbo.Messages", "SequenceNumber");
            this.RenameSequence("dbo.MessageSid", "MessageSequence");
            this.AddDefaultContraint("dbo.Messages", "SequenceNumber", "NEXT VALUE FOR [dbo].[MessageSequence]");

            RenameColumn("dbo.CouponLogs", "Sid", "SequenceNumber");
            RenameIndex("dbo.CouponLogs", "IX_Sid", "IX_SequenceNumber");
            this.DeleteDefaultContraint("dbo.CouponLogs", "SequenceNumber");
            this.RenameSequence("dbo.CouponLogSid", "CouponLogSequence");
            this.AddDefaultContraint("dbo.CouponLogs", "SequenceNumber", "NEXT VALUE FOR [dbo].[CouponLogSequence]");

            RenameColumn("dbo.SteamBots", "Sid", "SequenceNumber");
            RenameIndex("dbo.SteamBots", "IX_Sid", "IX_SequenceNumber");
            this.DeleteDefaultContraint("dbo.SteamBots", "SequenceNumber");
            this.RenameSequence("dbo.SteamBotSid", "SteamBotSequence");
            this.AddDefaultContraint("dbo.SteamBots", "SequenceNumber", "NEXT VALUE FOR [dbo].[SteamBotSequence]");

            RenameColumn("dbo.ArticleComments", "SidForArticle", "SequenceNumberForArticle");
            RenameIndex("dbo.ArticleComments", "IX_SidForArticle", "IX_SequenceNumberForArticle");

            RenameColumn("dbo.ArticleComments", "Sid", "SequenceNumber");
            RenameIndex("dbo.ArticleComments", "IX_Sid", "IX_SequenceNumber");
            this.DeleteDefaultContraint("dbo.ArticleComments", "SequenceNumber");
            this.RenameSequence("dbo.ArticleCommentSid", "CommentSequence");
            this.AddDefaultContraint("dbo.ArticleComments", "SequenceNumber", "NEXT VALUE FOR [dbo].[CommentSequence]");

            RenameColumn("dbo.KeylolUsers", "Sid", "SequenceNumber");
            RenameIndex("dbo.KeylolUsers", "IX_Sid", "IX_SequenceNumber");
            this.DeleteDefaultContraint("dbo.KeylolUsers", "SequenceNumber");
            this.RenameSequence("dbo.UserSid", "UserSequence");
            this.AddDefaultContraint("dbo.KeylolUsers", "SequenceNumber", "NEXT VALUE FOR [dbo].[UserSequence]");

            RenameColumn("dbo.Articles", "SidForAuthor", "SequenceNumberForAuthor");
            RenameIndex("dbo.Articles", "IX_SidForAuthor", "IX_SequenceNumberForAuthor");

            RenameColumn("dbo.Articles", "Sid", "SequenceNumber");
            RenameIndex("dbo.Articles", "IX_Sid", "IX_SequenceNumber");
            this.DeleteDefaultContraint("dbo.Articles", "SequenceNumber");
            this.RenameSequence("dbo.ArticleSid", "ArticleSequence");
            this.AddDefaultContraint("dbo.Articles", "SequenceNumber", "NEXT VALUE FOR [dbo].[ArticleSequence]");

            RenameTable("dbo.Replies", "CommentReplies");

            AddForeignKey("dbo.CommentReplies", "ReplyId", "dbo.ArticleComments", "Id");
            AddForeignKey("dbo.CommentReplies", "CommentId", "dbo.ArticleComments", "Id");
        }
    }
}