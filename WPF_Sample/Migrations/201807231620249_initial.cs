namespace WPF_Sample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Position = c.Int(nullable: false),
                        LogoLink = c.String(unicode: false),
                        MatchCount = c.Int(nullable: false),
                        Won = c.Int(nullable: false),
                        Tie = c.Int(nullable: false),
                        Lost = c.Int(nullable: false),
                        Score = c.String(unicode: false),
                        Points = c.Int(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        TotalMatches = c.Int(nullable: false),
                        Points = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Teams", "User_Id", "dbo.Users");
            DropIndex("dbo.Teams", new[] { "User_Id" });
            DropTable("dbo.Users");
            DropTable("dbo.Teams");
        }
    }
}
