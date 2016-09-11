namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CouponGiftPreviewImageNotRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CouponGifts", "PreviewImage", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CouponGifts", "PreviewImage", c => c.String(nullable: false));
        }
    }
}
