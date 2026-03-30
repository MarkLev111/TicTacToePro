using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace TicTacToePro
{
    internal class Stats
    {
        public int games { get; set; } = 0;
        private int Xwins { get; set; } = 0;
        private int Owins { get; set; } = 0;
        private int draws { get; set; } = 0;

        public Stats()
        {
            Stats stats = JsonSerializer.Deserialize<Stats>(File.ReadAllText("stats.json"));
            this.games = stats.games;
            this.Xwins = stats.Xwins;
            this.Owins = stats.Owins;
            this.draws = stats.draws;
        }

        public void AddGame(char result)
        {
            this.games++;
            if (result == 'X')
                this.Xwins++;
            else if (result == 'O')
                this.Owins++;
            else if (result == 'N')
                this.draws++;
            WriteStats();
        }

        public void WriteStats()
        {
            File.WriteAllText("stats.json", JsonSerializer.Serialize(this));
        }
    }
}
