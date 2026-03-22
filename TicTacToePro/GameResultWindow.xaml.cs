using System.Windows;
using TicTacToePro.Shared;

namespace TicTacToePro
{
    public partial class GameResultWindow : Window
    {
        public PostGameAction action { get; set; }
        public GameResultWindow()
        {
            action = PostGameAction.None;
            InitializeComponent();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            action = PostGameAction.NewGame;
            this.DialogResult = true;
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            action = PostGameAction.GoToMenu;
            this.DialogResult = true;
            Menu menu = new Menu();
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
