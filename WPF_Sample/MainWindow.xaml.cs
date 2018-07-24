using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_Sample.Model;
using System.Data.Entity;
using WPF_Sample.Scraper;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Data.Entity.Infrastructure;
using MaterialDesignThemes.Wpf;

namespace WPF_Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            EnsureCreated();

            RefreshTables();

            LoadChampionGrids();

                  
               
            

    

          
        }



        private void Update_Click(object sender, RoutedEventArgs e)
        {
            //https://elegantcode.com/2011/10/07/extended-wpf-toolkitusing-the-busyindicator/
            //using backgroundworker because the operations cannot be done on the UI thread
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, ea) =>
            {
                UpdateTeamStats();
            };

            //since you can't access UI elements on a seperate thread it needs to be done on the ui thread
            //currently the loading box is delayed until the dispatcher's code is done - not sure if there is a workaround for this
            Dispatcher.Invoke((Action)(() =>
            {
                using (var context = new Tournament())
                {
                    UpdateStanding();
                    var players = context.Users.Include(x => x.Teams).OrderByDescending(x => x.Points).ToList();

                    RefreshDataGrids(players);     
                }
            }));

            worker.RunWorkerCompleted += (o, ea) =>
            {
                ProgressIndicator.IsBusy = false;

                //snackbar indicating the tournament was successfully updated
                var myMessageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(3000));
                MySnackbar.MessageQueue = myMessageQueue;
                MySnackbar.MessageQueue.Enqueue("Stillingen blev opdateret!");
            };

            ProgressIndicator.IsBusy = true;
            worker.RunWorkerAsync();




        }

        private void RefreshDataGrids(List<User> players)
        {
            //setting itemsource to null and new items will refresh the datagrid
            //clear old values
            this.Standing.ItemsSource = null;
            this.Player1Grid.ItemsSource = null;
            this.Player2Grid.ItemsSource = null;
            this.Player3Grid.ItemsSource = null;
            this.Player4Grid.ItemsSource = null;
            this.Player5Grid.ItemsSource = null;
            this.Player6Grid.ItemsSource = null;
            this.Player7Grid.ItemsSource = null;
            this.Player8Grid.ItemsSource = null;
            this.Player9Grid.ItemsSource = null;
            this.Player10Grid.ItemsSource = null;

            //update with new values
            this.Standing.ItemsSource = players;
            this.Player1Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Anders").Teams;
            this.Player2Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Fælles 1").Teams;
            this.Player3Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Fælles 2").Teams;
            this.Player4Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Trix").Teams;
            this.Player5Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Sølvkær").Teams;
            this.Player6Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "CC").Teams;
            this.Player7Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Kirke").Teams;
            this.Player8Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Jakes").Teams;
            this.Player9Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Heine").Teams;
            this.Player10Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Ulrik").Teams;

        }

        private void LoadChampionGrids()
        {
            using (var context = new Tournament())
            {
                //this is equivalent of using thenInclude in ef core (this is ef6)
                var players = context.Users.Include(p => p.ChampionTeams.Select(x => x.WinningTeam)).ToList();

                this.Player1ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Anders").ChampionTeams;
                this.Player2ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Fælles 1").ChampionTeams;
                this.Player3ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Fælles 2").ChampionTeams;
                this.Player4ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Trix").ChampionTeams;
                this.Player5ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Sølvkær").ChampionTeams;
                this.Player6ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "CC").ChampionTeams;
                this.Player7ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Kirke").ChampionTeams;
                this.Player8ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Jakes").ChampionTeams;
                this.Player9ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Heine").ChampionTeams;
                this.Player10ChampGrid.ItemsSource = players.SingleOrDefault(x => x.Name == "Ulrik").ChampionTeams;
            }

        }

            private void UpdateTeamStats()
        {
            var bold = new BoldScraper();
            bold.UpdateTeamStats();
        }

        private void EnsureCreated()
        {
            using (var context = new Tournament())
            {
                context.Database.Initialize(true);
                context.Database.CreateIfNotExists();
                SeedPlayers();
            }
        }

        private void UpdateStanding()
        {
            using (var context = new Tournament())
            {
                var players = context.Users.Include(x => x.Teams).ToList();

                foreach (var player in players)
                {
                    var totalMatches = 0;
                    var totalLeagueMatches = 0;
                    var points = 0;
                    var totalGoals = 0;
                    var avgPoints = 0m;

                    //loop through player's teams and accumulate stats
                    foreach (var team in player.Teams)
                    {
                        totalMatches += team.MatchCount;
                        totalLeagueMatches += team.LeagueTotalMatches;
                        points += team.Points;
                        
                        if(team.Score != "0")
                        {
                            var splitted = team.Score.Split('-');
                            totalGoals += Int32.Parse(splitted[0]);
                        }
                    }

                    var remainingMatches = totalLeagueMatches - totalMatches;

                    if(points > 0)
                    {
                        avgPoints = (totalMatches/10m / points/10m) * 100m;
                    }
                    var maxPointsPossible = points + (remainingMatches * 3);

                    player.TotalMatches = totalMatches;
                    player.Points = points;
                    player.AvgPoints = decimal.Round(avgPoints, 2, MidpointRounding.AwayFromZero);
                    player.TotalGoals = totalMatches;
                    player.MaxPointsPossible = maxPointsPossible;
                    player.RemainingMatches = remainingMatches;

                    DbEntityEntry<User> entry = context.Entry(player);
                    entry.State = EntityState.Modified;

                }

                context.SaveChanges();

            }
        }

        private void RefreshTables()
        {
            using (var context = new Tournament())
            {
                var players = context.Users.Include(x => x.Teams).OrderByDescending(x => x.Points).ToList();

                //main standing
                this.Standing.ItemsSource = players;

                //players
                this.Player1Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Anders").Teams;
                this.Player2Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Fælles 1").Teams;
                this.Player3Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Fælles 2").Teams;
                this.Player4Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Trix").Teams;
                this.Player5Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Sølvkær").Teams;
                this.Player6Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "CC").Teams;
                this.Player7Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Kirke").Teams;
                this.Player8Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Jakes").Teams;
                this.Player9Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Heine").Teams;
                this.Player10Grid.ItemsSource = players.SingleOrDefault(x => x.Name == "Ulrik").Teams;
            }

        }

        private void SeedPlayers()
        {
            using (var context = new Tournament())
            {
                if (!context.Users.Any())
                {                  
                    //players
                    var players = new List<User>();

                    var Anders = new User()
                    {
                        Name = "Anders",
                        Teams = new List<Team>()
                    };
                    players.Add(Anders);

                    var Fælles1 = new User()
                    {
                        Name = "Fælles 1",
                        Teams = new List<Team>()
                    };
                    players.Add(Fælles1);

                    var Fælles2 = new User()
                    {
                        Name = "Fælles 2",
                        Teams = new List<Team>()
                    };
                    players.Add(Fælles2);

                    var Trix = new User()
                    {
                        Name = "Trix",
                        Teams = new List<Team>()
                    };
                    players.Add(Trix);

                    var Sølvkær = new User()
                    {
                        Name = "Sølvkær",
                        Teams = new List<Team>()
                    };
                    players.Add(Sølvkær);

                    var CC = new User()
                    {
                        Name = "CC",
                        Teams = new List<Team>()
                    };
                    players.Add(CC);

                    var Kirke = new User()
                    {
                        Name = "Kirke",
                        Teams = new List<Team>()
                    };
                    players.Add(Kirke);

                    var Jakes = new User()
                    {
                        Name = "Jakes",
                        Teams = new List<Team>()
                    };
                    players.Add(Jakes);

                    var Heine = new User()
                    {
                        Name = "Heine",
                        Teams = new List<Team>()
                    };
                    players.Add(Heine);

                    var Ulrik = new User()
                    {
                        Name = "Ulrik",
                        Teams = new List<Team>()
                    };
                    players.Add(Ulrik);

                    context.Users.AddRange(players);
                    context.SaveChanges();
                }
            }
        }
    }
}
