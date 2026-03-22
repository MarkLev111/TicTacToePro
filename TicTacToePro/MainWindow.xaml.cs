using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TicTacToePro.Shared;

namespace TicTacToePro
{
    public partial class MainWindow : Window
    {
        private Game? game;
        private Button[,] buttons;
        private HubConnection? connection;


        // Singleplayer settings


        public MainWindow() // singleplayer
        {
            InitializeComponent();
            game = new Game();
            buttons = new Button[9, 9];
            connection = null;
            CreateBoard();
            UpdateUI(this.game); // Отрисовываем начальное состояние
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

        private async void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            var pos = (Tuple<int, int>)clickedButton.Tag;
            int row = pos.Item1;
            int col = pos.Item2;

            if (this.game is Game) // СИНГЛ ПЛЕЕР
            {

                // Передаем ход в класс логики
                char result = game.Move(row, col);
                if (result == '-')
                    return;

                // Обновляем интерфейс после хода
                UpdateUI(this.game);

                if (result == 'X' || result == 'O' || result == 'N')
                {
                    GameResultWindow endGame = new GameResultWindow();
                    endGame.ResultText.Text = endGame.WinnerText(result);

                    if (endGame.ShowDialog() == true)
                    {
                        //game = new Game();
                        //UpdateUI(this.game);
                        GameResultAction(endGame.action); // проверить метод
                    }
                }
            }
            else // МУЛЬТИ ПЛЕЕР
            {
                await connection.SendAsync("Move", row, col); // инвоук ждёт ответ, сенд тупо шлёт
            }
        }

        // Синхронизация интерфейса с данными из класса Game
        // подумать сделать здесь такую штуку: можно кидать переменную, и это будет game / mpGame в зависимости от игры
        private void UpdateUI(Game game)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    // 1. Рисуем X и O
                    char cellValue = game.field[row, col]; // получаем Х/О
                    buttons[row, col].Content = cellValue == '\0' ? "" : cellValue.ToString();
                    buttons[row, col].Foreground = cellValue == 'X' ? Brushes.Red : Brushes.Blue;

                    // 2. Подсвечиваем доступные для хода клетки
                    int bigFieldPos = game.BigFieldPos(row, col);
                    char bigFieldValue = game.bigField[bigFieldPos / 10, bigFieldPos % 10];

                    if (bigFieldValue != '\0')
                        // Если большое поле уже выиграно, закрашиваем его целиком
                        buttons[row, col].Background = bigFieldValue == 'X' ? Brushes.LightPink : Brushes.LightBlue;
                    else if (!game.GetMyTurn() && game.nextMove == bigFieldPos)
                        buttons[row, col].Background = Brushes.Gray;
                    else if ((game.nextMove == -1 && !game.CheckFieldClosed(bigFieldPos)) || game.nextMove == bigFieldPos)
                        // Доступные для хода поля подсвечиваем зеленым
                        buttons[row, col].Background = Brushes.LightGreen;
                    else
                        // Недоступные поля оставляем белыми
                        buttons[row, col].Background = Brushes.White;
                }
            }
            if (game is Game)
                WindowTitle(game.XO);
        }

        public void WindowTitle(bool XO)
        {
            if (XO)
                WindowName.Title = $"TicTacToePro — X";
            else
                WindowName.Title = $"TicTacToePro — O";
        }


        // Multiplayer settings

        public MainWindow(bool checker) // кнопка на мультиплеер будет передавать значение и будет вызываться этот метод
        {
            InitializeComponent();
            buttons = new Button[9, 9];

            HubConnectionBuilder connectionBuilder = new HubConnectionBuilder();
            connectionBuilder.WithUrl("http://localhost:5195/gamehub"); // сервер
            connectionBuilder.WithAutomaticReconnect();
            connection = connectionBuilder.Build();

            // connection.On <ТИП ДАННЫХ> ("СЕРВЕРНЫЙ МЕТОД", (ПЕРЕМЕННАЯ) =>
            // {
            //      что делать
            // });

            connection.On<MoveInfo>("Move", (data) =>
            {
                Dispatcher.Invoke(() => Move(data)); // только внутренний код может трогать свой UI
            });

            connection.On<bool>("CreateGame", (XO) => // отправка пакета может быть другой !!!
            {
                Dispatcher.Invoke(() =>
                {
                    this.game = new MultiplayerGame(XO);
                    WindowTitle(XO);
                    UpdateUI(this.game);
                });
            });

            connection.On("EndGame", async () => await connection.StopAsync());

            Connect();

            CreateBoard();
        }

        private async void Connect()
        {
            try
            {
                await this.connection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к серверу.");
            }
        }

        public void Move(MoveInfo data)
        {
            game.field[data.row, data.column] = data.XOToPut; // просто постфактум изменение символа
            if (data.bigFieldChange != '\0')
                game.bigField[data.bigFieldPos / 10, data.bigFieldPos % 10] = data.bigFieldChange;

            UpdateUI(this.game);

            if (data.result == 'X' || data.result == 'O' || data.result == 'N') // подумать, как можно это вынести в отдельный метод
            {
                GameResultWindow endGame = new GameResultWindow();
                endGame.ResultText.Text = endGame.WinnerText(data.result);

                if (endGame.ShowDialog() == true)
                {
                    game = new Game();
                    UpdateUI(this.game);
                }
            }
        }

        public void GameResultAction(PostGameAction action) // проверить метод
        {
            switch (action)
            {
                case (PostGameAction.NewGame):
                    game = new Game();
                    UpdateUI(this.game);
                    return;
                case (PostGameAction.GoToMenu):
                    this.DialogResult = true;
                    Menu menu = new Menu();
                    return;
                default:
                    return;
            }
        }
    }
}