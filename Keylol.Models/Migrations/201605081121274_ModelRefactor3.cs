namespace Keylol.Models.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ModelRefactor3 : DbMigration
    {
        private void RecalculateSid(string tableName, string sequenceName, string orderColumn)
        {
            Sql(
                $@"ALTER SEQUENCE {sequenceName} RESTART
                    DECLARE @id nvarchar(128)
                    DECLARE [cursor] CURSOR FOR SELECT Id FROM {tableName} ORDER BY {orderColumn}
                    OPEN [cursor]
                    FETCH NEXT FROM [cursor] into @id
                    WHILE @@FETCH_STATUS = 0
                    BEGIN
	                    UPDATE {tableName} SET [Sid] = NEXT VALUE FOR {sequenceName} WHERE Id = @id
	                    FETCH NEXT FROM [cursor] INTO @id
                    END
                    CLOSE [cursor]   
                    DEALLOCATE [cursor]");
        }

        public override void Up()
        {
            DropForeignKey("dbo.Favorites", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.PointStaffs", "NormalPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.PointStaffs", "KeylolUser_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.Likes", "ArticleId", "dbo.Articles");
            DropForeignKey("dbo.Likes", "CommentId", "dbo.ArticleComments");
            DropForeignKey("dbo.UserPointSubscriptions", "KeylolUser_Id", "dbo.KeylolUsers");
            DropForeignKey("dbo.ProfilePoints", "Id", "dbo.KeylolUsers");

            DropIndex("dbo.Favorites", new[] {"AddTime"});
            DropIndex("dbo.Favorites", new[] {"UserId"});
            DropIndex("dbo.PointStaffs", new[] {"NormalPoint_Id"});
            DropIndex("dbo.PointStaffs", new[] {"KeylolUser_Id"});
            DropIndex("dbo.UserPointSubscriptions", new[] {"KeylolUser_Id"});
            DropIndex("dbo.ProfilePoints", new[] {"Id"});

            RenameTable("dbo.NormalPoints", "Points");

            this.CreateSequence("PointSid");
            this.CreateSequence("SteamStoreNameSid");
            this.CreateSequence("LogSid");
            this.CreateSequence("SteamBindingTokenSid");
            this.CreateSequence("ReplySid");
            this.CreateSequence("CouponGiftOrderSid");
            this.CreateSequence("CouponGiftSid");
            this.CreateSequence("LikeSid");
            this.CreateSequence("SubscriptionSid");

            AddColumn("dbo.KeylolUsers", "BackgroundImage", c => c.String(nullable: false, maxLength: 256));
            AddColumn("dbo.KeylolUsers", "PreferredPointName", c => c.Int(nullable: false));
            AddColumn("dbo.Points", "Sid",
                c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[PointSid]"));
            AddColumn("dbo.SteamStoreNames", "Sid",
                c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SteamStoreNameSid]"));
            AddColumn("dbo.Logs", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[LogSid]"));
            AddColumn("dbo.SteamBindingTokens", "Sid",
                c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SteamBindingTokenSid]"));
            AddColumn("dbo.Replies", "Sid",
                c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[ReplySid]"));
            AddColumn("dbo.CouponGiftOrders", "Sid",
                c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[CouponGiftOrderSid]"));
            AddColumn("dbo.CouponGifts", "Sid",
                c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[CouponGiftSid]"));
            AddColumn("dbo.Likes", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[LikeSid]"));
            AddColumn("dbo.Likes", "TargetType", c => c.Int(nullable: false));
            AddColumn("dbo.Likes", "TargetId", c => c.String(nullable: false, maxLength: 128));

            // 根据其他列重新计算 Sid
            RecalculateSid("dbo.Points", "PointSid", "CreateTime");
            RecalculateSid("dbo.Logs", "LogSid", "Time");
            RecalculateSid("dbo.CouponGiftOrders", "CouponGiftOrderSid", "RedeemTime");
            RecalculateSid("dbo.CouponGifts", "CouponGiftSid", "CreateTime");
            RecalculateSid("dbo.Likes", "LikeSid", "Time");

            // 将 Likes 的 ArticleId 和 CommentId 合并为 TargetId + TargetType
            Sql(@"WITH cte AS (SELECT Id, ArticleId FROM Likes WHERE ArticleId IS NOT NULL)
                UPDATE Likes SET TargetId = cte.ArticleId, TargetType = 0 FROM cte WHERE Likes.Id = cte.Id");
            Sql(@"WITH cte AS (SELECT Id, CommentId FROM Likes WHERE CommentId IS NOT NULL)
                UPDATE Likes SET TargetId = cte.CommentId, TargetType = 1 FROM cte WHERE Likes.Id = cte.Id");

            DropIndex("dbo.Likes", new[] {"ArticleId"});
            DropIndex("dbo.Likes", new[] {"CommentId"});

            // 将 ProfilePoints.BackgroundImage 移至 KeylolUsers.BackgroundImage
            Sql(@"WITH cte AS (
	                SELECT Id, BackgroundImage FROM ProfilePoints
                ) UPDATE KeylolUsers SET BackgroundImage = cte.BackgroundImage FROM cte WHERE KeylolUsers.Id = cte.Id");

            CreateTable(
                "dbo.Subscriptions",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Sid = c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[SubscriptionSid]"),
                    Time = c.DateTime(nullable: false),
                    SubscriberId = c.String(nullable: false, maxLength: 128),
                    TargetType = c.Int(nullable: false),
                    TargetId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.KeylolUsers", t => t.SubscriberId)
                .Index(t => t.Sid, unique: true, clustered: true)
                .Index(t => t.SubscriberId)
                .Index(t => t.TargetId);

            // 将 UserPointSubscriptions 迁移至 Subscriptions
            Sql(@"WITH cte AS (
	            SELECT KeylolUser_Id, Point_Id FROM UserPointSubscriptions
	                INNER JOIN Points ON Points.Id = Point_Id
                ) INSERT INTO Subscriptions (Id, SubscriberId, TargetId, TargetType, [Time])
                SELECT LOWER(NEWID()) AS Id, KeylolUser_Id AS SubscriberId, Point_Id AS TargetId,
                0 AS TargetType, GETDATE() AS [Time]
                FROM cte");
            Sql(@"WITH cte AS (
	            SELECT KeylolUser_Id, Point_Id FROM UserPointSubscriptions
	                INNER JOIN KeylolUsers ON KeylolUsers.Id = Point_Id
                ) INSERT INTO Subscriptions (Id, SubscriberId, TargetId, TargetType, [Time])
                SELECT LOWER(NEWID()) AS Id, KeylolUser_Id AS SubscriberId, Point_Id AS TargetId,
                1 AS TargetType, GETDATE() AS [Time]
                FROM cte");

            AlterColumn("dbo.KeylolUsers", "AvatarImage", c => c.String(nullable: false, maxLength: 256));
            CreateIndex("dbo.Points", "Sid", unique: true);
            CreateIndex("dbo.SteamStoreNames", "Sid", unique: true);
            CreateIndex("dbo.Replies", "Sid", unique: true);
            CreateIndex("dbo.CouponGiftOrders", "Sid", unique: true);
            CreateIndex("dbo.CouponGifts", "Sid", unique: true);
            CreateIndex("dbo.Logs", "Sid", unique: true);
            CreateIndex("dbo.Likes", "Sid", unique: true);
            CreateIndex("dbo.Likes", "TargetId");
            CreateIndex("dbo.SteamBindingTokens", "Sid", unique: true);

            // 需要人工在 SSMS 中设置以下 Sid 为 Clustered Index
            // Points.Sid
            // SteamStoreNames.Sid
            // Replies.Sid
            // CouponGiftOrders.Sid
            // CouponGifts.Sid
            // Logs.Sid
            // Likes.Sid
            // SteamBindingTokens.Sid

            Sql(@"IF OBJECT_ID('[FK_dbo.Likes_dbo.Entries_ArticleId]', 'F') IS NOT NULL
                ALTER TABLE dbo.Likes DROP CONSTRAINT [FK_dbo.Likes_dbo.Entries_ArticleId]");
            Sql(@"IF OBJECT_ID('[FK_dbo.Likes_dbo.Comments_CommentId]', 'F') IS NOT NULL
                ALTER TABLE dbo.Likes DROP CONSTRAINT [FK_dbo.Likes_dbo.Comments_CommentId]");

            DropColumn("dbo.KeylolUsers", "AutoSubscribeEnabled");
            DropColumn("dbo.KeylolUsers", "AutoSubscribeDaySpan");
            DropColumn("dbo.Points", "PreferredName");
            DropColumn("dbo.Likes", "ArticleId");
            DropColumn("dbo.Likes", "CommentId");
            DropColumn("dbo.Likes", "Discriminator");

            DropTable("dbo.Favorites");
            DropTable("dbo.PointStaffs");
            DropTable("dbo.UserPointSubscriptions");
            DropTable("dbo.ProfilePoints");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.ProfilePoints",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    BackgroundImage = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.UserPointSubscriptions",
                c => new
                {
                    KeylolUser_Id = c.String(nullable: false, maxLength: 128),
                    Point_Id = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new {t.KeylolUser_Id, t.Point_Id});

            CreateTable(
                "dbo.PointStaffs",
                c => new
                {
                    NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                    KeylolUser_Id = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new {t.NormalPoint_Id, t.KeylolUser_Id});

            CreateTable(
                "dbo.Favorites",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    AddTime = c.DateTime(nullable: false),
                    UserId = c.String(nullable: false, maxLength: 128),
                    PointId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Likes", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Likes", "CommentId", c => c.String(maxLength: 128));
            AddColumn("dbo.Likes", "ArticleId", c => c.String(maxLength: 128));
            AddColumn("dbo.Points", "PreferredName", c => c.Int(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoSubscribeDaySpan", c => c.Int(nullable: false));
            AddColumn("dbo.KeylolUsers", "AutoSubscribeEnabled", c => c.Boolean(nullable: false));

            DropForeignKey("dbo.Subscriptions", "SubscriberId", "dbo.KeylolUsers");
            DropIndex("dbo.Subscriptions", new[] {"TargetId"});
            DropIndex("dbo.Subscriptions", new[] {"SubscriberId"});
            DropIndex("dbo.Subscriptions", new[] {"Sid"});
            DropIndex("dbo.SteamBindingTokens", new[] {"Sid"});
            DropIndex("dbo.Likes", new[] {"TargetId"});
            DropIndex("dbo.Likes", new[] {"Sid"});
            DropIndex("dbo.Logs", new[] {"Sid"});
            DropIndex("dbo.CouponGifts", new[] {"Sid"});
            DropIndex("dbo.CouponGiftOrders", new[] {"Sid"});
            DropIndex("dbo.Replies", new[] {"Sid"});
            DropIndex("dbo.SteamStoreNames", new[] {"Sid"});
            DropIndex("dbo.Points", new[] {"Sid"});

            DropTable("dbo.Subscriptions");

            AlterColumn("dbo.KeylolUsers", "AvatarImage", c => c.String(nullable: false, maxLength: 64));
            DropColumn("dbo.Likes", "TargetId");
            DropColumn("dbo.Likes", "TargetType");
            DropColumn("dbo.Likes", "Sid");
            DropColumn("dbo.CouponGifts", "Sid");
            DropColumn("dbo.CouponGiftOrders", "Sid");
            DropColumn("dbo.Replies", "Sid");
            DropColumn("dbo.SteamBindingTokens", "Sid");
            DropColumn("dbo.Logs", "Sid");
            DropColumn("dbo.SteamStoreNames", "Sid");
            DropColumn("dbo.Points", "Sid");
            DropColumn("dbo.KeylolUsers", "PreferredPointName");
            DropColumn("dbo.KeylolUsers", "BackgroundImage");

            this.DropSequence("PointSid");
            this.DropSequence("SteamStoreNameSid");
            this.DropSequence("LogSid");
            this.DropSequence("SteamBindingTokenSid");
            this.DropSequence("ReplySid");
            this.DropSequence("CouponGiftOrderSid");
            this.DropSequence("CouponGiftSid");
            this.DropSequence("LikeSid");
            this.DropSequence("SubscriptionSid");

            RenameTable(name: "dbo.Points", newName: "NormalPoints");

            CreateIndex("dbo.ProfilePoints", "Id");
            CreateIndex("dbo.UserPointSubscriptions", "KeylolUser_Id");
            CreateIndex("dbo.PointStaffs", "KeylolUser_Id");
            CreateIndex("dbo.PointStaffs", "NormalPoint_Id");
            CreateIndex("dbo.Likes", "CommentId");
            CreateIndex("dbo.Likes", "ArticleId");
            CreateIndex("dbo.Favorites", "UserId");
            CreateIndex("dbo.Favorites", "AddTime");

            AddForeignKey("dbo.ProfilePoints", "Id", "dbo.KeylolUsers", "Id");
            AddForeignKey("dbo.UserPointSubscriptions", "KeylolUser_Id", "dbo.KeylolUsers", "Id");
            AddForeignKey("dbo.Likes", "CommentId", "dbo.ArticleComments", "Id");
            AddForeignKey("dbo.Likes", "ArticleId", "dbo.Articles", "Id");
            AddForeignKey("dbo.PointStaffs", "KeylolUser_Id", "dbo.KeylolUsers", "Id");
            AddForeignKey("dbo.PointStaffs", "NormalPoint_Id", "dbo.NormalPoints", "Id");
            AddForeignKey("dbo.Favorites", "UserId", "dbo.KeylolUsers", "Id");
        }
    }
}