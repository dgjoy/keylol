namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CouponLogCreateTimeIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.CouponLogs", "CreateTime");
        }
        
        public override void Down()
        {
            DropIndex("dbo.CouponLogs", new[] { "CreateTime" });
        }
    }
}
