namespace WPF_Sample
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using WPF_Sample.Model;

    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class Tournament : DbContext
    {
        // Your context has been configured to use a 'Tournament' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'WPF_Sample.Tournament' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'Tournament' 
        // connection string in the application configuration file.
        public Tournament()
            : base("name=Tournament")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<Tournament>());
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<ChampionGuess> ChampionGuesses { get; set; }

    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}