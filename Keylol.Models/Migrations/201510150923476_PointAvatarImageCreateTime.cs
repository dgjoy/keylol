namespace Keylol.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PointAvatarImageCreateTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NormalPoints", "AvatarImage", c => c.String(nullable: false, maxLength: 64));
            AddColumn("dbo.NormalPoints", "CreateTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NormalPoints", "CreateTime");
            DropColumn("dbo.NormalPoints", "AvatarImage");
        }
    }
}
