using System.Windows;

namespace TicTacToePro
{
    public partial class GameResultWindow : Window
    {
        public GameResultWindow()
        {
            InitializeComponent();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
