using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TicTacToePro.Shared;

namespace TicTacToeProServer
{
    class GameHub : Hub // заимствование класса из сигнала
    // любой публичный метод автоматом ухо
    {
        private static List<string> idsInQueue = new List<string>(); // мб заменить на ConcurrentQueue
        private static ConcurrentDictionary<string, Game> playersInGame = new ConcurrentDictionary<string, Game>();

        //private static List<Game> activeGames = new List<Game>(); // когда будет несколько игр, сюда буду их складывать
        // понять, надо ли оно вообще, если у меня есть playersInGame

        private readonly ILogger<GameHub> logger; // всё пишем в логи ради азура
        private readonly IHubContext<GameHub> hubContext; // ради передачи времени

        public GameHub(ILogger<GameHub> logger, IHubContext<GameHub> hubContext)
        {
            this.logger = logger;
            this.hubContext = hubContext;
        }

        public override async Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            idsInQueue.Add(id);

            logger.LogInformation($"> Установление новое подключение: {id}");

            await base.OnConnectedAsync();

            if (idsInQueue.Count >= 2)
                await this.CreateGame();
        }

        public async Task Authorize(string id)
        {

        }

        public async Task CreateGame()
        {
            string first = idsInQueue.First();
            idsInQueue.Remove(first);
            string second = idsInQueue.First();
            idsInQueue.Remove(second);

            Game game = new Game(first, second, hubContext);

            //activeGames.Add(game);

            playersInGame.TryAdd(first, game);
            playersInGame.TryAdd(second, game);

            await Groups.AddToGroupAsync(game.X, $"{game.X}{game.O}");
            await Groups.AddToGroupAsync(game.O, $"{game.X}{game.O}");

            game.time.Start();

            await Clients.Client(game.X).SendAsync("CreateGame", true); // отправить выполнение MultiplayerGame с Х/О
            await Clients.Client(game.O).SendAsync("CreateGame", false);
            // try-catch

            logger.LogInformation($"> Создана игра: {game.X} / {game.O}");
        }

        public async Task Move(int row, int column)
        {
            //Console.WriteLine($"Совершён ход {row},{column}");

            string id = Context.ConnectionId; // отправитель

            Game game = null;
            playersInGame.TryGetValue(id, out game);
            if (game == null)
                throw new Exception("Игрок с таким айди не играет в данный момент");

            char result;
            lock (game)
            {
                result = game.Move(id, row, column);
            }
            if (result == '-') // до игрока даже не дойдёт эта команда, она остановится на сервере
                // мб то, что ниже, даже не важно, потому что такие коды обрабатываются в бездну на стороне клиента
                return;
            else
            {
                int bigFieldPos = game.BigFieldPos(row, column);
                MoveInfo data = new MoveInfo(row, column, game.field[row, column], game.nextMove, result, bigFieldPos, game.bigField[bigFieldPos / 10, bigFieldPos % 10]);
                await Clients.Group($"{game.X}{game.O}").SendAsync("Move", data);

                logger.LogInformation($"> В игру {game.X} / {game.O} отправлен корректный ход {row},{column}");

                if (result != '.')
                {
                    DisconnectedAction action = DisconnectedAction.Normal;
                    EndGame(game); // мне нужно полное удаление игры
                    await Clients.Client(game.X).SendAsync("EndGame", action);
                    await Clients.Client(game.O).SendAsync("EndGame", action);

                    logger.LogInformation($"> Игра {game.X} / {game.O} завершена, игроки отключены");
                }
            }
        }

        public async Task EndGame(Game game) // убрать их из своего списка игроков
        {
            playersInGame.TryRemove(game.X, out _);
            playersInGame.TryRemove(game.O, out _);

            await Groups.RemoveFromGroupAsync(game.X, $"{game.X}{game.O}");
            await Groups.RemoveFromGroupAsync(game.O, $"{game.X}{game.O}");
        }

        public async override Task OnDisconnectedAsync(Exception? exception) // сделать окно закрытия игры, когда типа просто отключило от сервера
        {
            logger.LogInformation($"> Разорвано подключение: {Context.ConnectionId}");

            Game game = null;
            if (idsInQueue.Contains(Context.ConnectionId))
            {

                idsInQueue.Remove(Context.ConnectionId);
            }
            else if (playersInGame.TryGetValue(Context.ConnectionId, out game)) // при окончании игры я удаляю в EndGame игру. если тип отключился сам, игра останется, второму придёт завершение
            {
                DisconnectedAction action = DisconnectedAction.Disconnect;
                await Clients.Group($"{game.X}{game.O}").SendAsync("EndGame", action); // ОТПРАВИТЬ ЕНАМ, ЧТО ИГРА БЫЛА ЗАВЕРШЕНА ВЫХОДОМ СОПЕРНИКА
                await EndGame(game); // мне НЕ нужно полное удаление игры, уже запущен метод дисконнекта

                logger.LogInformation($"> Игра {game.X} / {game.O} завершена принудительно ({Context.ConnectionId} отключился)");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}