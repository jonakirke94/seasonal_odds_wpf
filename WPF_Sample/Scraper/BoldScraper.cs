using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;using OpenQA.Selenium.Chrome;
using WPF_Sample.Model;

namespace WPF_Sample.Scraper
{
    public class BoldScraper
    {

        private const string Bundesliga = "https://www.bold.dk/fodbold/tyskland/bundesliga/";
        private const string Championship = "https://www.bold.dk/fodbold/england/championship/";
        private const string SuperLig = "https://www.bold.dk/fodbold/tyrkiet/super-lig/";
        private const string LaLiga = "https://www.bold.dk/fodbold/tyrkiet/super-lig/";
        private const string Æresdivisionen = "https://www.bold.dk/fodbold/holland/aeresdivisionen/";
        private const string SerieA = "https://www.bold.dk/fodbold/italien/serie-a/";
        private const string NordicBetLiga = "https://www.bold.dk/fodbold/danmark/nordicbet-liga/";
        private const string PremierLeague = "https://www.bold.dk/fodbold/england/premier-league/";
        private const string SuperLigaen = "https://www.bold.dk/fodbold/danmark/superligaen/";
        private const string Ligue1 = "https://www.bold.dk/fodbold/frankrig/ligue-1/";



        private const string CurrentSeason = "18/19";

        private List<string> LeagueUrls { get; set; }

        public BoldScraper()
        {
            LeagueUrls = new List<string>
            {
                Bundesliga,
                Championship,
                SuperLig,
                LaLiga,
                Æresdivisionen,
                SerieA,
                NordicBetLiga,
                PremierLeague,
                SuperLigaen,
                Ligue1
            };

        }


        //make private static later
        private static HtmlDocument LoadHtmlPage(string url, bool requireBrowser = false)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // For https calls
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // Blindly accept certificates (DANGER AHEAD)

            HtmlDocument result;

            if (!requireBrowser)
            {
                var web = new HtmlWeb
                {
                    //this encoding allows the reader to read Æ/Ø/Å
                    OverrideEncoding = Encoding.GetEncoding("ISO-8859-1")
                };

                result = web.Load(url);
            }
            else
            {
                var options = new ChromeOptions();
                options.AddArgument("--headless");
                var driver = new ChromeDriver(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Driver", "V2.33", "chromedriver_win32.exe"),
                    options)
                {
                    Url = url
                };

                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(2);

                //driver.Navigate().GoToUrl(url);
                var container = driver.FindElementById("tournament");
                var html = container.GetAttribute("innerHTML");
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                result = doc;
            }

            return result;
        }

        public void UpdateTeamStats()
        {
            foreach (var url in LeagueUrls)
            {
                var doc = LoadHtmlPage(url);
                var isActive = IsLeagueActive(doc);
                UpdateLeague(doc, isActive);
            }
        }


        //this method saves team statistics for a league or updates it if it already exists
        private void UpdateLeague(HtmlDocument doc, bool isActive)
        {
            using (var context = new Tournament())
            {
                var teams = ScrapeLeague(doc);
                var newTeams = new List<Team>();

                foreach (var team in teams)
                {
                    Team existingTeam = GetTeamByName(team.Name);

                    if(existingTeam == null)
                    {
                        //add team to list of new teams to be added
                        newTeams.Add(team);

                    } else
                    {
                        //if team already exists the teams running stats are updated given that the season is active

                        //Stats to update: Position - MatchCount - Won - Tie - Lost - Score - Points

                        if (isActive)
                        {
                            existingTeam.Position = team.Position;
                            existingTeam.MatchCount = team.MatchCount;
                            existingTeam.Won = team.Won;
                            existingTeam.Tie = team.Tie;
                            existingTeam.Lost = team.Lost;
                            existingTeam.Score = team.Score;
                            existingTeam.Points = team.Points;
                        } else
                        {
                            existingTeam.Position = 1;
                            existingTeam.MatchCount = 0;
                            existingTeam.Won = 0;
                            existingTeam.Tie = 0;
                            existingTeam.Lost = 0;
                            existingTeam.Score = "0-0";
                            existingTeam.Points = 0;
                        }

                        DbEntityEntry<Team> entry = context.Entry(existingTeam);
                        entry.State = EntityState.Modified;
                    }
                }

                if(newTeams.Count > 0)
                {
                    context.Teams.AddRange(teams);
                }


                context.SaveChanges();
            }
        }

        //this method determines whether the league has started or shows results from the previous season
        private Boolean IsLeagueActive(HtmlDocument doc)
        {
      
            bool isActive = false;

            var container = doc.GetElementbyId("tournament_structure_header");
            var title_box = container.SelectSingleNode("//*[contains(@class,'title_box_container')]");
            var title_text = title_box.Descendants("h1").First().InnerText;
            
            if(title_text.Contains(CurrentSeason))
            {
                isActive = true;
            }

            return isActive;
        }

        private Team GetTeamByName(string name)
        {
            using (var context = new Tournament())
            {
                return context.Teams.FirstOrDefault(x => x.Name == name);
            }
        }

        private static List<Team> ScrapeLeague(HtmlDocument doc)
        {
            //var doc = LoadHtmlPage(url);
            var container = doc.DocumentNode.SelectNodes("//*[contains(@class,'sub_page')]").First();
            var table = container.Descendants("table").First();
            var td = table.SelectNodes("tr");

            //league
            var league = container.Descendants("h1").First().InnerHtml;

            //total matches
            var leagueInfoNode = td[0].SelectNodes("td");
            var infoText = leagueInfoNode[0].Descendants("span").First().InnerHtml;
            var totalMatches = Regex.Match(infoText, "[0-9]+").ToString();

            var teams = new List<Team>();

            //each team
            foreach (var item in td.Skip(2))
            {
               teams.Add(ScrapeTeam(item, league, totalMatches));
            }

            return teams;
        }

        private static Team ScrapeTeam(HtmlNode team, string league, string totalMatches)
        {
            //each data cell
            var tds = team.SelectNodes("td");

            //position
            var position = tds[0].FirstChild.InnerHtml;

            //logolink
            var imageNode = tds[1].SelectSingleNode(".//img");
            var logoLink = "https:" + imageNode.Attributes["src"].Value;

            //clubname
            var clubNode = tds[2].FirstChild;
            //var club = clubNode.SelectNodes("//*[contains(@class,'team_name_container')]").First().InnerText;
            var clubName = clubNode.Descendants("div").First().Descendants("div").First().InnerText;

            //matchcount
            var matchCount = tds[3].FirstChild.InnerHtml;

            //won
            var won = tds[4].FirstChild.InnerHtml;

            //tie
            var ties = tds[5].FirstChild.InnerHtml;

            //lost
            var lost = tds[6].FirstChild.InnerHtml;

            //score TODO: format
            var scoreText = tds[7].FirstChild.InnerHtml;
            var matches = Regex.Matches(scoreText, "[0-9]+");
            var score = matches[0] + "-" + matches[1];

            //points
            var points = tds[8].FirstChild.InnerHtml;

            return new Team()
            {
                LeagueName = league,
                LeagueTotalMatches = Int32.Parse(totalMatches),
                Position = Int32.Parse(position),
                LogoLink = logoLink,
                Name = clubName,
                MatchCount = Int32.Parse(matchCount),
                Won = Int32.Parse(won),
                Tie = Int32.Parse(ties),
                Lost = Int32.Parse(lost),
                Score = score,
                Points = Int32.Parse(points)
            };

            //Debug.WriteLine(league + " " + totalMatches + " " + position + " " + clubName + " " + matchCount + " " + won + " " + ties + " " + lost + " " + score + " " + points);
        }
    }
}
