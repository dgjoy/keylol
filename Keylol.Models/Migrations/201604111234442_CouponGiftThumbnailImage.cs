namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CouponGiftThumbnailImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CouponGifts", "ThumbnailImage", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CouponGifts", "ThumbnailImage");
        }
    }
}
