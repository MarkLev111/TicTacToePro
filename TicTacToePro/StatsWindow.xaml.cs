using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TicTacToePro.Shared;
using static System.Collections.Specialized.BitVector32;

namespace TicTacToePro
{
    /// <summary>
    /// Логика взаимодействия для StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        public StatsWindow()
        {
            InitializeComponent();
            this.Games.Text = $"Всего игр: {Stats.currentStats?.games}";

            int XwinPercent = 0;
            int OwinPercent = 0;
            int drawPercent = 0;
            if (Stats.currentStats?.games != 0)
            {
                XwinPercent = Stats.currentStats.Xwins * 100 / Stats.currentStats.games;
                OwinPercent = Stats.currentStats.Owins * 100 / Stats.currentStats.games;
                drawPercent = Stats.currentStats.draws * 100 / Stats.currentStats.games;
            }
            this.X.Text = $"Побед X: {Stats.currentStats?.Xwins} ({XwinPercent}%)";
            this.O.Text = $"Побед O: {Stats.currentStats?.Owins} ({OwinPercent}%)";
            this.Draw.Text = $"Ничейных игр: {Stats.currentStats?.draws} ({drawPercent}%)";

            MultiplayerStats stats = Authorize.GetStats(this).Result;
            if (!string.IsNullOrEmpty(stats.errorMessage))
            {
                this.Error.Text = stats.errorMessage;
            }
            else
            {
                this.MPGames.Text = $"Всего игр: {stats.games}";
                int MPwinPercent = 0;
                int MPlosePercent = 0;
                int MPdrawPercent = 0;
                if (stats.games != 0)
                {
                    MPwinPercent = stats.wins * 100 / stats.games;
                    MPlosePercent = stats.loses * 100 / stats.games;
                    MPdrawPercent = stats.draws * 100 / stats.games;
                }
                this.X.Text = $"Побед: {stats.wins} ({MPwinPercent}%)";
                this.O.Text = $"Поражений {stats.loses} ({MPlosePercent}%)";
                this.Draw.Text = $"Ничейных игр: {stats.draws} ({MPdrawPercent}%)";
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Menu menu = new Menu();
            menu.Show();
            this.Close();
        }
    }
}
