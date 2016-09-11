namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyCouponGiftOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CouponGiftOrders", "RedeemPrice", c => c.Int(nullable: false));
            DropColumn("dbo.CouponGiftOrders", "CurrentPrice");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CouponGiftOrders", "CurrentPrice", c => c.Int(nullable: false));
            DropColumn("dbo.CouponGiftOrders", "RedeemPrice");
        }
    }
}
