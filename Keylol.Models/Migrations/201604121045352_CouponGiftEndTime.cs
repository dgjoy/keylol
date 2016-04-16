namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CouponGiftEndTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CouponGifts", "EndTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CouponGifts", "EndTime");
        }
    }
}
