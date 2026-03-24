using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TicTacToePro.Shared;
using System.Timers;

namespace TicTacToePro
{
    public partial class MainWindow : Window
    {
        public event Action ReadyToWork;

        private Game? game;
        private Button[,] buttons;
        private HubConnection? connection;
        private bool gameInProgress = true;
        private System.Timers.Timer time = new System.Timers.Timer(1000);
        private int seconds = 0;


        // Singleplayer settings


        public MainWindow() // singleplayer
        {
            InitializeComponent();

            game = new Game();
            buttons = new Button[9, 9];
            connection = null;
            time.AutoReset = true;
            time.Elapsed += OnTimerTick;

            CreateBoard();
            UpdateUI(this.game); // Отрисовываем начальное состояние

            time.Start();

            ReadyToWork?.Invoke();
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


            if (connection == null) // СИНГЛ ПЛЕЕР
            {

                // Передаем ход в класс логики
                char result = game.Move(row, col);
                if (result == '-')
                    return;

                // Обновляем интерфейс после хода
                UpdateUI(this.game);

                if (result == 'X' || result == 'O' || result == 'N')
                    GameResultWindow(result);
            }
            else // МУЛЬТИ ПЛЕЕР
            {
                if (this.game == null)
                    return;

                if (!this.game.GetMyTurn())
                    return;
                try
                {
                    await connection.SendAsync("Move", row, col); // инвоук ждёт ответ, сенд тупо шлёт
                }
                catch
                {
                    return;
                }
            }
        }

        // Синхронизация интерфейса с данными из класса Game
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
                    else if (this.game is MultiplayerGame && game.nextMove == -1 && !game.GetMyTurn()) // если приходит отбивка -1 по правилам
                        buttons[row, col].Background = Brushes.White;
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
            string timer = $"{seconds / 60}:{seconds % 60}";
            if (seconds % 60 < 10)
                timer = $"{seconds / 60}:0{seconds % 60}";
            if (seconds / 60 < 10)
                timer = "0" + timer;

            if (XO)
                WindowName.Title = $"TicTacToePro — X — {timer}";
            else
                WindowName.Title = $"TicTacToePro — O — {timer}";
        }


        // Multiplayer settings

        public MainWindow(Object o) // кнопка на мультиплеер будет передавать значение и будет вызываться этот метод
        {
            InitializeComponent();

            buttons = new Button[9, 9];
            time = null;

            HubConnectionBuilder connectionBuilder = new HubConnectionBuilder();
            connectionBuilder.WithUrl("https://tictactoepro-a6egbyh8ake9cgdv.israelcentral-01.azurewebsites.net/gamehub"); // сервер
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

            connection.On<DisconnectedAction>("EndGame", async (action) =>
            {
                await connection.StopAsync();
                if (action == DisconnectedAction.Disconnect) // ЭТО ЗНАЧИТ, ЧТО СОПЕРНИК ДИСКОННЕКТНУЛСЯ
                {
                    Dispatcher.Invoke(() => GameResultWindow('D'));
                }
            });

            Connect();

            CreateBoard();
        }

        private async void Connect()
        {
            try
            {
                await this.connection.StartAsync();

                ReadyToWork?.Invoke();
            }
            catch (Exception ex)
            {
                // сделать так, чтобы при неудачном подключении к серверу выбрасывалась ошибка и главное меню
                MessageBox.Show("Не удалось подключиться к серверу.");
            }
        }

        public void Move(MoveInfo data)
        {
            game.field[data.row, data.column] = data.XOToPut; // просто постфактум изменение символа
            if (data.bigFieldChange != '\0')
                game.bigField[data.bigFieldPos / 10, data.bigFieldPos % 10] = data.bigFieldChange;

            game.nextMove = data.nextMove;
            game.SetMyTurn(); // меняем в UI, что сейчас чужой ход

            UpdateUI(this.game);

            if (data.result == 'X' || data.result == 'O' || data.result == 'N') // подумать, как можно это вынести в отдельный метод
            {
                GameResultWindow(data.result);
            }
        }

        public void GameResultAction(PostGameAction action)
        {
            time = new System.Timers.Timer(1000);
            switch (action)
            {
                case (PostGameAction.NewGame): // сделать параметр Multiplayer / Singleplayer
                    if (this.game is MultiplayerGame)
                    {
                        Multiplayer();
                        return;
                    }
                    else
                    {
                        game = new Game();
                        seconds = 0;
                        time.AutoReset = true;
                        time.Elapsed += OnTimerTick;
                        UpdateUI(this.game);
                        time.Start();
                        return;
                    }
                case (PostGameAction.GoToMenu):
                    Menu menu = new Menu();
                    //menu.ReadyToWork += () => this.Close();
                    menu.Show();
                    this.Close();
                    return;
                default:
                    return;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (gameInProgress)
            {
                this.time.Dispose();
                base.OnClosed(e);
                Application.Current.Shutdown();
            }
            else
                return;
        }

        public void Multiplayer()
        {
            MainWindow window = new MainWindow(true);
            window.ReadyToWork += () => this.Close();
            window.Show();
        }

        public void GameResultWindow(char result)
        {
            if (this.game is not MultiplayerGame)
                this.time.Dispose();

            gameInProgress = false;
            GameResultWindow endGame = new GameResultWindow();
            endGame.ResultText.Text = endGame.WinnerText(result);

            if (endGame.ShowDialog() == true)
                GameResultAction(endGame.action);
        }

        public void OnTimerTick(Object? o, ElapsedEventArgs? e)
        {
            this.seconds++;
            Dispatcher.Invoke(() =>
            {
                WindowTitle(this.game.XO);
            });
        }
    }
}