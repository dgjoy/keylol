namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CouponLogUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CouponLogs", "UserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.CouponLogs", "Description", c => c.String(nullable: false));
            CreateIndex("dbo.CouponLogs", "UserId");
            AddForeignKey("dbo.CouponLogs", "UserId", "dbo.KeylolUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CouponLogs", "UserId", "dbo.KeylolUsers");
            DropIndex("dbo.CouponLogs", new[] { "UserId" });
            AlterColumn("dbo.CouponLogs", "Description", c => c.String());
            DropColumn("dbo.CouponLogs", "UserId");
        }
    }
}
