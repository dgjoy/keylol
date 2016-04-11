namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CouponGifts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CouponGiftOrders",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        GiftId = c.String(nullable: false, maxLength: 128),
                        RedeemTime = c.DateTime(nullable: false),
                        Extra = c.String(nullable: false),
                        Finished = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CouponGifts", t => t.GiftId)
                .ForeignKey("dbo.KeylolUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.GiftId)
                .Index(t => t.RedeemTime);
            
            CreateTable(
                "dbo.CouponGifts",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                        Descriptions = c.String(nullable: false),
                        PreviewImage = c.String(nullable: false),
                        AcceptedFields = c.String(nullable: false),
                        Price = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.CreateTime);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CouponGiftOrders", "UserId", "dbo.KeylolUsers");
            DropForeignKey("dbo.CouponGiftOrders", "GiftId", "dbo.CouponGifts");
            DropIndex("dbo.CouponGifts", new[] { "CreateTime" });
            DropIndex("dbo.CouponGiftOrders", new[] { "RedeemTime" });
            DropIndex("dbo.CouponGiftOrders", new[] { "GiftId" });
            DropIndex("dbo.CouponGiftOrders", new[] { "UserId" });
            DropTable("dbo.CouponGifts");
            DropTable("dbo.CouponGiftOrders");
        }
    }
}
