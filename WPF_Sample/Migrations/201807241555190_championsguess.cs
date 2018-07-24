namespace WPF_Sample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class championsguess : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChampionGuesses", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ChampionGuesses", "WinningTeam_Id", "dbo.Teams");
            DropIndex("dbo.ChampionGuesses", new[] { "User_Id" });
            DropIndex("dbo.ChampionGuesses", new[] { "WinningTeam_Id" });
            DropTable("dbo.ChampionGuesses");
        }
    }
}
