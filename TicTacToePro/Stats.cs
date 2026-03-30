using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace TicTacToePro
{
    internal class Stats
    {
        public int games { get; set; } = 0;
        public int Xwins { get; set; } = 0;
        public int Owins { get; set; } = 0;
        public int draws { get; set; } = 0;
        private static string jsonPath = GetJsonPath();
        public static Stats? currentStats { get; set; }

        public Stats()
        {
            currentStats = this;
        }

        public static void ReadJson()
        {
            if (!File.Exists(jsonPath)) // если файла нет, создаём новый
            {
                File.WriteAllText(jsonPath, JsonSerializer.Serialize(currentStats));
            }
            else // а если есть, читаем существующий
            {
                currentStats = JsonSerializer.Deserialize<Stats>(File.ReadAllText(jsonPath));
            }
        }

        public void AddGame(char result)
        {
            currentStats.games++;
            if (result == 'X')
                currentStats.Xwins++;
            else if (result == 'O')
                currentStats.Owins++;
            else if (result == 'N')
                currentStats.draws++;
            WriteStats();
        }

        private void WriteStats()
        {
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(currentStats));
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
