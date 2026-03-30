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
        private static string jsonPath = GetJsonPath();

        public Stats()
        {
            if (!File.Exists(jsonPath)) // если файла нет, создаём новый
            {
                File.WriteAllText(jsonPath, JsonSerializer.Serialize(this));
            }
            else // а если есть, читаем существующий
            {
                Stats? stats = JsonSerializer.Deserialize<Stats>(File.ReadAllText(jsonPath));
                this.games = stats.games;
                this.Xwins = stats.Xwins;
                this.Owins = stats.Owins;
                this.draws = stats.draws;
            }
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

        private void WriteStats()
        {
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(this));
        }

        private static string GetJsonPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // AppData
            string gameFolder = Path.Combine(appDataPath, "TicTacToePro"); // ищем путь к папке игры
            Directory.CreateDirectory(gameFolder);
            string statsJson = Path.Combine(gameFolder, "stats.json");
            return statsJson;
        }
    }
}
