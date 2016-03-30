namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Coupon : DbMigration
    {
        public override void Up()
        {
            Sql("CREATE SEQUENCE [dbo].[CouponLogSequence] AS int START WITH 1 INCREMENT BY 1 NO CACHE");
            CreateTable(
                "dbo.CouponLogs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        SequenceNumber = c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[CouponLogSequence]"),
                        Event = c.Int(nullable: false),
                        Change = c.Int(nullable: false),
                        Balance = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .Index(t => t.SequenceNumber, unique: true, clustered: true);
            
            AddColumn("dbo.KeylolUsers", "Coupon", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropIndex("dbo.CouponLogs", new[] { "SequenceNumber" });
            DropColumn("dbo.KeylolUsers", "Coupon");
            DropTable("dbo.CouponLogs");
        }
    }
}
