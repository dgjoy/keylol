namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCurrentPrice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CouponGiftOrders", "CurrentPrice", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CouponGiftOrders", "CurrentPrice");
        }
    }
}
