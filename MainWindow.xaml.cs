using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TicTacToePro
{
    public partial class MainWindow : Window
    {
        private Game game;
        private Button[,] buttons;

        public MainWindow()
        {
            InitializeComponent();
            game = new Game();
            buttons = new Button[9, 9];
            CreateBoard();
            UpdateUI(); // Отрисовываем начальное состояние
        }

        // Генерация игрового поля
        private void CreateBoard()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    Button btn = new Button
                    {
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        Background = Brushes.White,
                        // Сохраняем координаты кнопки прямо в неё, чтобы использовать при клике
                        Tag = new Tuple<int, int>(row, col)
                    };

                    // Делаем границы между большими клетками (3х3) толще
                    double top = (row % 3 == 0) ? 4 : 1;
                    double left = (col % 3 == 0) ? 4 : 1;
                    double bottom = (row == 8) ? 4 : 1;
                    double right = (col == 8) ? 4 : 1;
                    btn.Margin = new Thickness(left, top, right, bottom);

                    // Подписываемся на событие клика
                    btn.Click += Cell_Click;

                    buttons[row, col] = btn;
                    GameGrid.Children.Add(btn);
                }
            }
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            var pos = (Tuple<int, int>)clickedButton.Tag;
            int row = pos.Item1;
            int col = pos.Item2;

            // Передаем ход в класс логики
            char result = game.Move(row, col);
            if (result == '-')
                return;

            if (result == 'X' || result == 'O' || result == 'N')
            {
                GameResultWindow endGame = new GameResultWindow();
                endGame.ResultText.Text = endGame.WinnerText(result);

                if (endGame.ShowDialog() == true)
                {
                    game = new Game();
                    UpdateUI();
                }
            }

            // Обновляем интерфейс после хода
            UpdateUI();
        }

        // Синхронизация интерфейса с данными из класса Game
        private void UpdateUI()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    // 1. Рисуем X и O
                    char cellValue = game.field[row, col];
                    buttons[row, col].Content = cellValue == '\0' ? "" : cellValue.ToString();
                    buttons[row, col].Foreground = cellValue == 'X' ? Brushes.Red : Brushes.Blue;

                    // 2. Подсвечиваем доступные для хода клетки
                    int bigFieldPos = game.BigFieldPos(row, col);
                    char bigFieldValue = game.bigField[bigFieldPos / 10, bigFieldPos % 10];

                    if (bigFieldValue != '\0')
                    {
                        // Если большое поле уже выиграно, закрашиваем его целиком
                        buttons[row, col].Background = bigFieldValue == 'X' ? Brushes.LightPink : Brushes.LightBlue;
                    }
                    else if (game.nextMove == -1 || game.nextMove == bigFieldPos)
                    {
                        // Доступные для хода поля подсвечиваем зеленым
                        buttons[row, col].Background = Brushes.LightGreen;
                    }
                    else
                    {
                        // Недоступные поля оставляем белыми
                        buttons[row, col].Background = Brushes.White;
                    }
                }
            }
        }
    }
}