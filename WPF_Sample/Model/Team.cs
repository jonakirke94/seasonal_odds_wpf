using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Sample.Model
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public string LogoLink { get; set; }
        public int MatchCount { get; set; }
        public int Won { get; set; }
        public int Tie { get; set; }
        public int Lost { get; set; }
        public string Score { get; set; }
        public int Points { get; set; }
        public string LeagueName { get; set; }
        public int LeagueTotalMatches { get; set; }

        public List<ChampionGuess> ChampionGuesses { get; set; }

    }
}
