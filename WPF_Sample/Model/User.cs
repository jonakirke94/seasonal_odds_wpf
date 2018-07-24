using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Sample.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int TotalMatches { get; set; }
        public int Points { get; set; }
        public decimal AvgPoints { get; set; }
        public int TotalGoals { get; set; }
        public int MaxPointsPossible { get; set; }
        public int RemainingMatches { get; set; }

        public List<Team> Teams { get; set; }
        public List<ChampionGuess> ChampionTeams { get; set; }

    }
}
