namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCouponGiftType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CouponGifts", "GiftType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CouponGifts", "GiftType");
        }
    }
}
