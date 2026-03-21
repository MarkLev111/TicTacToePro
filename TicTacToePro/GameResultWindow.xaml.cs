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

        public string WinnerText(char winner)
        {
            switch (winner)
            {
                case 'X':
                    return "Победили крестики!";
                case 'O':
                    return "Победили нолики!";
                case 'N':
                    return "Ничья!";
                default:
                    throw new ArgumentOutOfRangeException("Такого результата игры нет");
            }
        }
    }
}
