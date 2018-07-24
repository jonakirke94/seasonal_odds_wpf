namespace WPF_Sample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class league : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teams", "LeagueName", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Teams", "LeagueName");
        }
    }
}
