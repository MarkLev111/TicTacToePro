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

namespace TicTacToePro
{
    public partial class Menu : Window
    {
        private bool progress { get; set; } = false;
        public Menu()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(Authorize.GetToken()))
            {
                this.Login.Visibility = Visibility.Hidden;
                this.Logout.Content = $"Выйти из аккаунта {Authorize.username}";
                this.Logout.Visibility = Visibility.Visible;
            }
        }

        public void Singleplayer(object sender, RoutedEventArgs e)
        {
            if (progress)
                return;
            progress = true;
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }
        public async void Multiplayer(object sender, RoutedEventArgs e)
        {
            if (progress)
                return;
            progress = true;

            MainWindow window = new MainWindow(true);
            window.ReadyToWork += () => this.Close();

            bool connect = await Authorize.MainWindowConnect(window);
            if (connect)
                window.Show();
            else
                window.Close();
        }

        public async void StatsWindow(object sender, RoutedEventArgs e)
        {
            if (progress)
                return;
            progress = true;
            MultiplayerStats stats = await Authorize.GetStats(this);

            StatsWindow window = new StatsWindow(stats);
            window.Show();
            this.Close();
        }

        public void Login_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }

        public void Logout_Click(object sender, RoutedEventArgs e)
        {
            Authorize.DeleteToken();
            Menu window = new Menu();
            window.Show();
            this.Close();
        }
    }
}