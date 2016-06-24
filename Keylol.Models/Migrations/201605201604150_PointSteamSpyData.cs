namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointSteamSpyData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Points", "OwnerCount", c => c.Int());
            AddColumn("dbo.Points", "OwnerCountVariance", c => c.Int());
            AddColumn("dbo.Points", "TotalPlayerCount", c => c.Int());
            AddColumn("dbo.Points", "TotalPlayerCountVariance", c => c.Int());
            AddColumn("dbo.Points", "TwoWeekPlayerCount", c => c.Int());
            AddColumn("dbo.Points", "TwoWeekPlayerCountVariance", c => c.Int());
            AddColumn("dbo.Points", "AveragePlayedTime", c => c.Int());
            AddColumn("dbo.Points", "TwoWeekAveragePlayedTime", c => c.Int());
            AddColumn("dbo.Points", "MedianPlayedTime", c => c.Int());
            AddColumn("dbo.Points", "TwoWeekMedianPlayedTime", c => c.Int());
            AddColumn("dbo.Points", "Ccu", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Points", "Ccu");
            DropColumn("dbo.Points", "TwoWeekMedianPlayedTime");
            DropColumn("dbo.Points", "MedianPlayedTime");
            DropColumn("dbo.Points", "TwoWeekAveragePlayedTime");
            DropColumn("dbo.Points", "AveragePlayedTime");
            DropColumn("dbo.Points", "TwoWeekPlayerCountVariance");
            DropColumn("dbo.Points", "TwoWeekPlayerCount");
            DropColumn("dbo.Points", "TotalPlayerCountVariance");
            DropColumn("dbo.Points", "TotalPlayerCount");
            DropColumn("dbo.Points", "OwnerCountVariance");
            DropColumn("dbo.Points", "OwnerCount");
        }
    }
}
