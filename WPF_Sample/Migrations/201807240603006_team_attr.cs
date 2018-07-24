namespace WPF_Sample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class team_attr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teams", "LeagueTotalMatches", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Teams", "LeagueTotalMatches");
        }
    }
}
