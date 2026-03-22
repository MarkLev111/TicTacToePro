using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using TicTacToePro.Shared;

namespace TicTacToeProServer
{
    class GameHub : Hub // заимствование класса из сигнала
        // любой публичный метод автоматом ухо
    {
        private static List<string> idsInQueue = new List<string>(); // мб заменить на ConcurrentQueue
        private static ConcurrentDictionary<string, Game> playersInGame = new ConcurrentDictionary<string, Game>();

        //    private static List<Game> activeGames = new List<Game>(); // когда будет несколько игр, сюда буду их складывать
        // понять, надо ли оно вообще, если у меня есть playersInGame

        // private static List<Player> playersInGame = new List<Player>();
        // меняю на словарь

        public override async Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            idsInQueue.Add(id);
            Console.WriteLine($"Установление новое подключение: {id}");

            await base.OnConnectedAsync();

            if (idsInQueue.Count >= 2)
                await this.CreateGame();
        }

        public async Task CreateGame()
        {
            string first = idsInQueue.First();
            idsInQueue.Remove(first);
            string second = idsInQueue.First();
            idsInQueue.Remove(second);

            Game game = new Game(first, second);

         //   activeGames.Add(game);

            playersInGame.TryAdd(first, game);
            playersInGame.TryAdd(second, game);

            await Groups.AddToGroupAsync(game.X, $"{game.X}{game.O}");
            await Groups.AddToGroupAsync(game.O, $"{game.X}{game.O}");


            await Clients.Client(game.X).SendAsync("CreateGame", true); // отправить выполнение MultiplayerGame с Х/О
            await Clients.Client(game.O).SendAsync("CreateGame", false);
            // try-catch

            Console.WriteLine($"Создана игра: {game.X} / {game.O}");
        }

        public async Task Move(int row, int column)
        {
            Console.WriteLine($"Совершён ход {row},{column}");

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
                return;
            else
            {
                int bigFieldPos = game.BigFieldPos(row, column);
                MoveInfo data = new MoveInfo(row, column, game.field[row, column], game.nextMove, result, bigFieldPos, game.bigField[bigFieldPos / 10, bigFieldPos % 10]);
                await Clients.Group($"{game.X}{game.O}").SendAsync("Move", data);

                Console.WriteLine($"В игру отправлен корректный ход {game.X} / {game.O}");

                if (result != '.')
                {
                    await Clients.Group($"{game.X}{game.O}").SendAsync("EndGame");
                    await EndGame(game);
                    Console.WriteLine($"Игра {game.X} / {game.O} завершена");
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
    }
}




// если у хода появился результат, его нужно передать обоим игрокам в формате нового сокращённого класса
// или просто параметрами. мб метод у клиента будет принимать от сервера эти параметры, а у него локальные обновлять.