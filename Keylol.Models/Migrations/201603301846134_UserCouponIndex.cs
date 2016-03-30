namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserCouponIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.KeylolUsers", "Coupon");
        }
        
        public override void Down()
        {
            DropIndex("dbo.KeylolUsers", new[] { "Coupon" });
        }
    }
}
