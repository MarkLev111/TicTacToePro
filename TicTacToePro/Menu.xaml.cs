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
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
        }

        public void Singleplayer(object sender, RoutedEventArgs e)
        {

            this.DialogResult = true;
        }
        public void Multiplayer(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
