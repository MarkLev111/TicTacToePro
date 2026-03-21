using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using TicTacToePro.Shared;

namespace TicTacToeProServer
{
    class GameHub : Hub // заимствование класса из сигнала
        // любой публичный метод автоматом ухо
    {
        private static List<string> idsInQueue = new List<string>();
    //    private static List<Game> activeGames = new List<Game>(); // когда будет несколько игр, сюда буду их складывать
        // понять, надо ли оно вообще, если у меня есть playersInGame
        private static List<Player> playersInGame = new List<Player>();

        public override async Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            idsInQueue.Add(id);

            base.OnConnectedAsync();

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
            playersInGame.Add(new Player (first, game));
            playersInGame.Add(new Player (second, game));

            await Groups.AddToGroupAsync(game.X, $"{game.X}{game.O}");
            await Groups.AddToGroupAsync(game.O, $"{game.X}{game.O}");


            await Clients.Client(game.X).SendAsync("CreateGame", true); // отправить выполнение MultiplayerGame с Х/О
            await Clients.Client(game.O).SendAsync("CreateGame", false);
            // try-catch
        }

        public async Task Move(int row, int column)
        {
            string id = Context.ConnectionId; // отправитель

            Game game = null;
            Player[] players = playersInGame.ToArray();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].id == id)
                {
                    game = players[i].game;
                    break;
                }
            }
            if (game == null)
                throw new Exception("Игра по айди не была найдена");

            char result = game.Move(id, row, column);
            if (result == '-') // до игрока даже не дойдёт эта команда, она остановится на сервере
                return;
            else
            {
                int bigFieldPos = game.BigFieldPos(row, column);
                MoveInfo data = new MoveInfo(row, column, game.field[row, column], game.nextMove, result, bigFieldPos, game.bigField[bigFieldPos / 10, bigFieldPos % 10]);
                await Clients.Group($"{game.X}{game.O}").SendAsync("Move", data);

                //await Clients.Client(game.X).SendAsync("Move", row, column, result); // настоить посыльщик
                //await Clients.Client(game.O).SendAsync("Move", row, column, result);
            }
        }

        public async void EndGame(Game game) // убрать их из своего списка игроков
        {
       //     activeGames.Remove(game);

            List<string> ids = new List<string>();
            Player[] players = playersInGame.ToArray();
            int counter = 0;
            for (int i = 0; i < players.Length; i++)
            {
                Player player = players[i];
                if (player.game == game) // проверить, что без .get получается
                {
                    ids.Add(player.id);
                    counter++;
                    playersInGame.Remove(player);
                }
                if (counter == 2)
                    break;
            }
            if (counter != 2)
                throw new Exception($"В игре было найдено {counter} человек");

            // Отправить обоим по айдишникам сообщение, что игра окончена

            await Groups.RemoveFromGroupAsync(game.X, $"{game.X}{game.O}");
            await Groups.RemoveFromGroupAsync(game.O, $"{game.X}{game.O}");
        }
    }
}



// если у хода появился результат, его нужно передать обоим игрокам в формате нового сокращённого класса
// или просто параметрами. мб метод у клиента будет принимать от сервера эти параметры, а у него локальные обновлять.