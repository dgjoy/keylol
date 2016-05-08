namespace Keylol.Models.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ModelRefactor1 : DbMigration
    {
        public override void Up()
        {
            RenameTable("dbo.Comments", "ArticleComments");

            DropForeignKey("dbo.ArticlePointPushes", "Article_Id", "dbo.Articles");
            DropForeignKey("dbo.ArticlePointPushes", "NormalPoint_Id", "dbo.NormalPoints");
            DropForeignKey("dbo.AutoSubscriptions", "NormalPointId", "dbo.NormalPoints");
            DropForeignKey("dbo.AutoSubscriptions", "UserId", "dbo.KeylolUsers");

            DropIndex("dbo.Articles", new[] {"Type"});
            DropIndex("dbo.Articles", new[] {"Archived"});
            DropIndex("dbo.Articles", new[] {"Rejected"});
            DropIndex("dbo.Articles", new[] {"SpotlightTime"});
            DropIndex("dbo.AutoSubscriptions", new[] {"UserId"});
            DropIndex("dbo.AutoSubscriptions", new[] {"NormalPointId"});
            DropIndex("dbo.AutoSubscriptions", new[] {"DisplayOrder"});
            DropIndex("dbo.ArticlePointPushes", new[] {"Article_Id"});
            DropIndex("dbo.ArticlePointPushes", new[] {"NormalPoint_Id"});

            Sql(@"IF OBJECT_ID('[FK_dbo.Entries_dbo.NormalPoints_VoteForPointId]', 'F') IS NOT NULL
                ALTER TABLE dbo.Articles DROP CONSTRAINT [FK_dbo.Entries_dbo.NormalPoints_VoteForPointId]");
            DropForeignKey("dbo.Articles", "VoteForPointId", "dbo.NormalPoints");
            RenameColumn("dbo.Articles", "VoteForPointId", "TargetPointId");
            RenameIndex("dbo.Articles", "IX_VoteForPointId", "IX_TargetPointId");
            AddForeignKey("dbo.Articles", "TargetPointId", "dbo.NormalPoints", "Id");

            Sql(@"IF OBJECT_ID('[FK_dbo.Entries_dbo.ProfilePoints_PrincipalId]', 'F') IS NOT NULL
                ALTER TABLE dbo.Articles DROP CONSTRAINT [FK_dbo.Entries_dbo.ProfilePoints_PrincipalId]");
            DropForeignKey("dbo.Articles", "PrincipalId", "dbo.ProfilePoints");
            RenameColumn("dbo.Articles", "PrincipalId", "AuthorId");
            RenameIndex("dbo.Articles", "IX_PrincipalId", "IX_AuthorId");
            AddForeignKey("dbo.Articles", "AuthorId", "dbo.KeylolUsers", "Id");

            RenameColumn("dbo.Articles", "IgnoreNewLikes", "DismissLikeMessage");
            RenameColumn("dbo.Articles", "IgnoreNewComments", "DismissCommentMessage");
            RenameColumn("dbo.Articles", "Vote", "Rating");

            AddColumn("dbo.Articles", "Subtitle", c => c.String(nullable: false, maxLength: 120));
            AddColumn("dbo.Articles", "AttachedPoints", c => c.String(nullable: false));
            AddColumn("dbo.Articles", "Deleted", c => c.Int(nullable: false));
            AddColumn("dbo.Articles", "Spotlighted", c => c.Boolean(nullable: false));

            // 迁移 ArticlePointPushes 表内容至 Articles.AttachedPoints 列
            const string attachedPointsSql = @"WITH Push(Article_Id, AttachedPoints) AS (
	                        SELECT
		                        Article_Id,
		                        '[""' + STUFF(
		                        (SELECT '"",""' + NormalPoint_Id
		                        FROM [dbo].[ArticlePointPushes] AS t3
		                        WHERE t3.Article_Id = t1.Article_Id AND (t2.TargetPointId IS NULL OR t3.NormalPoint_Id != t2.TargetPointId)
		                        FOR XML PATH('')), 1, 3, '') + '""]' AS AttachedPoints
	                        FROM [dbo].[ArticlePointPushes] AS t1
	                        INNER JOIN [dbo].[Articles] AS t2 ON t2.Id = Article_Id
	                        GROUP BY Article_Id, TargetPointId
                        ) UPDATE [dbo].[Articles] SET AttachedPoints = (
	                        CASE WHEN Push.AttachedPoints IS NULL THEN '[]'
		                        ELSE Push.AttachedPoints
	                        END
                        ), TargetPointId = (CASE WHEN TargetPointId IS NULL THEN SUBSTRING(Push.AttachedPoints, 3, 36) ELSE TargetPointId END)
                        FROM Push WHERE Articles.Id = Push.Article_Id";
            Sql(attachedPointsSql);
            Sql(attachedPointsSql); // 需要执行两次，确保 TargetPointId 和 AttachedPoints 满足约束要求

            // 迁移 Articles.SpotlightTime 至 Articles.Spotlighted 列
            Sql(@"WITH SpotlightArticles(Id) AS (
	                SELECT Id FROM [dbo].[Articles] WHERE SpotlightTime IS NOT NULL
                ) UPDATE [dbo].[Articles] SET Spotlighted = 'True' FROM SpotlightArticles WHERE Articles.Id = SpotlightArticles.Id");

            RenameColumn("dbo.ArticleComments", "IgnoreNewLikes", "DismissLikeMessage");
            RenameColumn("dbo.ArticleComments", "IgnoreNewComments", "DismissReplyMessage");

            AddColumn("dbo.ArticleComments", "Deleted", c => c.Int(nullable: false));

            DropColumn("dbo.Articles", "Type");
            DropColumn("dbo.Articles", "UnstyledContent");
            DropColumn("dbo.Articles", "SpotlightTime");

            DropTable("dbo.AutoSubscriptions");
            DropTable("dbo.ArticlePointPushes");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.ArticlePointPushes",
                c => new
                {
                    Article_Id = c.String(nullable: false, maxLength: 128),
                    NormalPoint_Id = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new {t.Article_Id, t.NormalPoint_Id});

            CreateTable(
                "dbo.AutoSubscriptions",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    UserId = c.String(nullable: false, maxLength: 128),
                    NormalPointId = c.String(nullable: false, maxLength: 128),
                    Type = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Articles", "SpotlightTime", c => c.DateTime());
            AddColumn("dbo.Articles", "UnstyledContent", c => c.String(nullable: false));
            AddColumn("dbo.Articles", "Type", c => c.Int(nullable: false));

            DropColumn("dbo.ArticleComments", "Deleted");

            RenameColumn("dbo.ArticleComments", "DismissReplyMessage", "IgnoreNewComments");
            RenameColumn("dbo.ArticleComments", "DismissLikeMessage", "IgnoreNewLikes");

            DropColumn("dbo.Articles", "Spotlighted");
            DropColumn("dbo.Articles", "Deleted");
            DropColumn("dbo.Articles", "AttachedPoints");
            DropColumn("dbo.Articles", "Subtitle");

            RenameColumn("dbo.Articles", "Rating", "Vote");
            RenameColumn("dbo.Articles", "DismissCommentMessage", "IgnoreNewComments");
            RenameColumn("dbo.Articles", "DismissLikeMessage", "IgnoreNewLikes");

            DropForeignKey("dbo.Articles", "AuthorId", "dbo.KeylolUsers");
            RenameColumn("dbo.Articles", "AuthorId", "PrincipalId");
            RenameIndex("dbo.Articles", "IX_AuthorId", "IX_PrincipalId");
            AddForeignKey("dbo.Articles", "PrincipalId", "dbo.ProfilePoints", "Id");

            DropForeignKey("dbo.Articles", "TargetPointId", "dbo.NormalPoints");
            RenameColumn("dbo.Articles", "TargetPointId", "VoteForPointId");
            RenameIndex("dbo.Articles", "IX_TargetPointId", "IX_VoteForPointId");
            AddForeignKey("dbo.Articles", "VoteForPointId", "dbo.NormalPoints", "Id");

            CreateIndex("dbo.ArticlePointPushes", "NormalPoint_Id");
            CreateIndex("dbo.ArticlePointPushes", "Article_Id");
            CreateIndex("dbo.AutoSubscriptions", "DisplayOrder");
            CreateIndex("dbo.AutoSubscriptions", "NormalPointId");
            CreateIndex("dbo.AutoSubscriptions", "UserId");
            CreateIndex("dbo.Articles", "SpotlightTime");
            CreateIndex("dbo.Articles", "Rejected");
            CreateIndex("dbo.Articles", "Archived");
            CreateIndex("dbo.Articles", "Type");

            AddForeignKey("dbo.AutoSubscriptions", "UserId", "dbo.KeylolUsers", "Id");
            AddForeignKey("dbo.AutoSubscriptions", "NormalPointId", "dbo.NormalPoints", "Id");
            AddForeignKey("dbo.ArticlePointPushes", "NormalPoint_Id", "dbo.NormalPoints", "Id");
            AddForeignKey("dbo.ArticlePointPushes", "Article_Id", "dbo.Articles", "Id");

            RenameTable(name: "dbo.ArticleComments", newName: "Comments");
        }
    }
}