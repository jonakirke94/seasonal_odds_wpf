namespace WPF_Sample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChampionGuesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WinningTeam_Id = c.Int(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Teams", t => t.WinningTeam_Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.WinningTeam_Id)
                .Index(t => t.User_Id);
            
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
                        LeagueName = c.String(unicode: false),
                        LeagueTotalMatches = c.Int(nullable: false),
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
                        AvgPoints = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalGoals = c.Int(nullable: false),
                        MaxPointsPossible = c.Int(nullable: false),
                        RemainingMatches = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Teams", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ChampionGuesses", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ChampionGuesses", "WinningTeam_Id", "dbo.Teams");
            DropIndex("dbo.Teams", new[] { "User_Id" });
            DropIndex("dbo.ChampionGuesses", new[] { "User_Id" });
            DropIndex("dbo.ChampionGuesses", new[] { "WinningTeam_Id" });
            DropTable("dbo.Users");
            DropTable("dbo.Teams");
            DropTable("dbo.ChampionGuesses");
        }
    }
}
