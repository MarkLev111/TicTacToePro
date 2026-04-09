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

namespace TicTacToePro
{
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(Authorize.GetToken()))
            {
                this.Login.Visibility = Visibility.Hidden;
                this.Logout.Content = "Выйти из аккаунта";
                this.Logout.Visibility = Visibility.Visible;
            }
        }

        public void Singleplayer(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            this.Close();
        }
        public void Multiplayer(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow(true);
            window.ReadyToWork += () => {
                window.Show();
                this.Close();
            };
        }

        public void StatsWindow(object sender, RoutedEventArgs e)
        {
            StatsWindow window = new StatsWindow();
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