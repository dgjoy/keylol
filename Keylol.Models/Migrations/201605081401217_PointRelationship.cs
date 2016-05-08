namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointRelationship : DbMigration
    {
        public override void Up()
        {
            // PointType.Hardware 由 5 改成 4
            Sql(@"UPDATE Points SET [Type] = 4 WHERE [Type] = 5");

            DropForeignKey("dbo.GameDeveloperPointAssociations", "GamePoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameDeveloperPointAssociations", "DeveloperPoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameGenrePointAssociations", "GamePoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameGenrePointAssociations", "GenrePoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameMajorPlatformPointAssociations", "GamePoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameMajorPlatformPointAssociations", "MajorPlatformPoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameMinorPlatformPointAssociations", "GamePoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameMinorPlatformPointAssociations", "MinorPlatformPoint_Id", "dbo.Points");
            DropForeignKey("dbo.GamePublisherPointAssociations", "GamePoint_Id", "dbo.Points");
            DropForeignKey("dbo.GamePublisherPointAssociations", "PublisherPoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameSeriesPointAssociations", "GamePoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameSeriesPointAssociations", "SeriesPoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameTagPointAssociations", "GamePoint_Id", "dbo.Points");
            DropForeignKey("dbo.GameTagPointAssociations", "TagPoint_Id", "dbo.Points");

            this.CreateSequence("PointRelationshipSid");
            CreateTable(
                "dbo.PointRelationships",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Sid = c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[PointRelationshipSid]"),
                    Relationship = c.Int(nullable: false),
                    SourcePointId = c.String(nullable: false, maxLength: 128),
                    TargetPointId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.Points", t => t.SourcePointId)
                .ForeignKey("dbo.Points", t => t.TargetPointId)
                .Index(t => t.Sid, unique: true, clustered: true)
                .Index(t => t.SourcePointId)
                .Index(t => t.TargetPointId);

            // 将据点关系全部移入 PointRelationships 表
            Sql(@"WITH cte AS (
                    SELECT GamePoint_Id, DeveloperPoint_Id FROM GameDeveloperPointAssociations
                ) INSERT INTO PointRelationships (Id, Relationship, SourcePointId, TargetPointId)
                SELECT LOWER(NEWID()) AS Id, 0 AS Relationship,
                GamePoint_Id AS SourcePointId, DeveloperPoint_Id AS TargetPointId FROM cte"); // 开发商
            Sql(@"WITH cte AS (
                    SELECT GamePoint_Id, PublisherPoint_Id FROM GamePublisherPointAssociations
                ) INSERT INTO PointRelationships (Id, Relationship, SourcePointId, TargetPointId)
                SELECT LOWER(NEWID()) AS Id, 1 AS Relationship,
                GamePoint_Id AS SourcePointId, PublisherPoint_Id AS TargetPointId FROM cte"); // 发行商
            Sql(@"WITH cte AS (
                    SELECT GamePoint_Id, GenrePoint_Id FROM GameGenrePointAssociations
                ) INSERT INTO PointRelationships (Id, Relationship, SourcePointId, TargetPointId)
                SELECT LOWER(NEWID()) AS Id, 3 AS Relationship,
                GamePoint_Id AS SourcePointId, GenrePoint_Id AS TargetPointId FROM cte"); // 流派
            Sql(@"WITH cte AS (
                    SELECT GamePoint_Id, SeriesPoint_Id FROM GameSeriesPointAssociations
                ) INSERT INTO PointRelationships (Id, Relationship, SourcePointId, TargetPointId)
                SELECT LOWER(NEWID()) AS Id, 4 AS Relationship,
                GamePoint_Id AS SourcePointId, SeriesPoint_Id AS TargetPointId FROM cte"); // 系列
            Sql(@"WITH cte AS (
                    SELECT GamePoint_Id, TagPoint_Id FROM GameTagPointAssociations
                ) INSERT INTO PointRelationships (Id, Relationship, SourcePointId, TargetPointId)
                SELECT LOWER(NEWID()) AS Id, 5 AS Relationship,
                GamePoint_Id AS SourcePointId, TagPoint_Id AS TargetPointId FROM cte"); // 特性
            Sql(@"WITH cte AS (
                    SELECT GamePoint_Id, MajorPlatformPoint_Id FROM GameMajorPlatformPointAssociations
                ) INSERT INTO PointRelationships (Id, Relationship, SourcePointId, TargetPointId)
                SELECT LOWER(NEWID()) AS Id, 6 AS Relationship,
                GamePoint_Id AS SourcePointId, MajorPlatformPoint_Id AS TargetPointId FROM cte"); // 主要平台
            Sql(@"WITH cte AS (
                    SELECT GamePoint_Id, MinorPlatformPoint_Id FROM GameMinorPlatformPointAssociations
                ) INSERT INTO PointRelationships (Id, Relationship, SourcePointId, TargetPointId)
                SELECT LOWER(NEWID()) AS Id, 7 AS Relationship,
                GamePoint_Id AS SourcePointId, MinorPlatformPoint_Id AS TargetPointId FROM cte"); // 次要平台

            DropIndex("dbo.GameDeveloperPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameDeveloperPointAssociations", new[] { "DeveloperPoint_Id" });
            DropIndex("dbo.GameGenrePointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameGenrePointAssociations", new[] { "GenrePoint_Id" });
            DropIndex("dbo.GameMajorPlatformPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameMajorPlatformPointAssociations", new[] { "MajorPlatformPoint_Id" });
            DropIndex("dbo.GameMinorPlatformPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameMinorPlatformPointAssociations", new[] { "MinorPlatformPoint_Id" });
            DropIndex("dbo.GamePublisherPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GamePublisherPointAssociations", new[] { "PublisherPoint_Id" });
            DropIndex("dbo.GameSeriesPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameSeriesPointAssociations", new[] { "SeriesPoint_Id" });
            DropIndex("dbo.GameTagPointAssociations", new[] { "GamePoint_Id" });
            DropIndex("dbo.GameTagPointAssociations", new[] { "TagPoint_Id" });
            
            DropTable("dbo.GameDeveloperPointAssociations");
            DropTable("dbo.GameGenrePointAssociations");
            DropTable("dbo.GameMajorPlatformPointAssociations");
            DropTable("dbo.GameMinorPlatformPointAssociations");
            DropTable("dbo.GamePublisherPointAssociations");
            DropTable("dbo.GameSeriesPointAssociations");
            DropTable("dbo.GameTagPointAssociations");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.GameTagPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        TagPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.TagPoint_Id });
            
            CreateTable(
                "dbo.GameSeriesPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        SeriesPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.SeriesPoint_Id });
            
            CreateTable(
                "dbo.GamePublisherPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        PublisherPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.PublisherPoint_Id });
            
            CreateTable(
                "dbo.GameMinorPlatformPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        MinorPlatformPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.MinorPlatformPoint_Id });
            
            CreateTable(
                "dbo.GameMajorPlatformPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        MajorPlatformPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.MajorPlatformPoint_Id });
            
            CreateTable(
                "dbo.GameGenrePointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        GenrePoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.GenrePoint_Id });
            
            CreateTable(
                "dbo.GameDeveloperPointAssociations",
                c => new
                    {
                        GamePoint_Id = c.String(nullable: false, maxLength: 128),
                        DeveloperPoint_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.GamePoint_Id, t.DeveloperPoint_Id });
            
            DropForeignKey("dbo.PointRelationships", "TargetPointId", "dbo.Points");
            DropForeignKey("dbo.PointRelationships", "SourcePointId", "dbo.Points");
            DropIndex("dbo.PointRelationships", new[] { "TargetPointId" });
            DropIndex("dbo.PointRelationships", new[] { "SourcePointId" });
            DropIndex("dbo.PointRelationships", new[] { "Sid" });
            DropTable("dbo.PointRelationships");
            this.DropSequence("PointRelationshipSid");
            CreateIndex("dbo.GameTagPointAssociations", "TagPoint_Id");
            CreateIndex("dbo.GameTagPointAssociations", "GamePoint_Id");
            CreateIndex("dbo.GameSeriesPointAssociations", "SeriesPoint_Id");
            CreateIndex("dbo.GameSeriesPointAssociations", "GamePoint_Id");
            CreateIndex("dbo.GamePublisherPointAssociations", "PublisherPoint_Id");
            CreateIndex("dbo.GamePublisherPointAssociations", "GamePoint_Id");
            CreateIndex("dbo.GameMinorPlatformPointAssociations", "MinorPlatformPoint_Id");
            CreateIndex("dbo.GameMinorPlatformPointAssociations", "GamePoint_Id");
            CreateIndex("dbo.GameMajorPlatformPointAssociations", "MajorPlatformPoint_Id");
            CreateIndex("dbo.GameMajorPlatformPointAssociations", "GamePoint_Id");
            CreateIndex("dbo.GameGenrePointAssociations", "GenrePoint_Id");
            CreateIndex("dbo.GameGenrePointAssociations", "GamePoint_Id");
            CreateIndex("dbo.GameDeveloperPointAssociations", "DeveloperPoint_Id");
            CreateIndex("dbo.GameDeveloperPointAssociations", "GamePoint_Id");
            AddForeignKey("dbo.GameTagPointAssociations", "TagPoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameTagPointAssociations", "GamePoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameSeriesPointAssociations", "SeriesPoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameSeriesPointAssociations", "GamePoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GamePublisherPointAssociations", "PublisherPoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GamePublisherPointAssociations", "GamePoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameMinorPlatformPointAssociations", "MinorPlatformPoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameMinorPlatformPointAssociations", "GamePoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameMajorPlatformPointAssociations", "MajorPlatformPoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameMajorPlatformPointAssociations", "GamePoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameGenrePointAssociations", "GenrePoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameGenrePointAssociations", "GamePoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameDeveloperPointAssociations", "DeveloperPoint_Id", "dbo.Points", "Id");
            AddForeignKey("dbo.GameDeveloperPointAssociations", "GamePoint_Id", "dbo.Points", "Id");
        }
    }
}
