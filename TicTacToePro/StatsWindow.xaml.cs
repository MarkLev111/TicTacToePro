using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public StatsWindow(MultiplayerStats stats)
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

            if (!string.IsNullOrEmpty(stats.errorMessage))
            {
                this.ErrorText.Text = stats.errorMessage;
                this.ErrorText.Visibility = Visibility.Visible;
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
                this.MPWins.Text = $"Побед: {stats.wins} ({MPwinPercent}%)";
                this.MPLoses.Text = $"Поражений {stats.loses} ({MPlosePercent}%)";
                this.MPDraw.Text = $"Ничейных игр: {stats.draws} ({MPdrawPercent}%)";
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Menu menu = new Menu();
            menu.Show();
            base.OnClosing(e);
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
