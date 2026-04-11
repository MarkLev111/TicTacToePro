using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToePro.Shared
{
    public class MultiplayerStats
    {
        public int Id { get; set; }
        public int games { get; set; }
        public int wins { get; set; }
        public int loses { get; set; }
        public int draws { get; set; }
        [NotMapped]
        public string errorMessage { get; set; } = string.Empty;

        public MultiplayerStats(int games, int wins, int loses, int draws) 
        { 
            this.games = games;
            this.wins = wins;
            this.loses = loses;
            this.draws = draws;
        }

        public MultiplayerStats(string serverMessage)
        {
            errorMessage = serverMessage;
        }

        public MultiplayerStats() { }
    }
}
