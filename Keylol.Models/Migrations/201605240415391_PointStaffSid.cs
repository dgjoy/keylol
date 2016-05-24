namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointStaffSid : DbMigration
    {
        public override void Up()
        {
            this.CreateSequence("PointStaffSid");
            AddColumn("dbo.PointStaffs", "Sid", c => c.Int(nullable: false, defaultValueSql: "NEXT VALUE FOR [dbo].[PointStaffSid]"));
            CreateIndex("dbo.PointStaffs", "Sid", unique: true, clustered: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.PointStaffs", new[] { "Sid" });
            DropColumn("dbo.PointStaffs", "Sid");
            this.DropSequence("PointStaffSid");
        }
    }
}
