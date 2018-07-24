namespace WPF_Sample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class user_attr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "AvgPoints", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Users", "TotalGoals", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "MaxPointsPossible", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "RemainingMatches", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "RemainingMatches");
            DropColumn("dbo.Users", "MaxPointsPossible");
            DropColumn("dbo.Users", "TotalGoals");
            DropColumn("dbo.Users", "AvgPoints");
        }
    }
}
