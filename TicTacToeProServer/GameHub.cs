using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicTacToePro.Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TicTacToeProServer
{
    [Authorize] // он вообще не пустит в класс по 401
    class GameHub : Hub // заимствование класса из сигнала
    // любой публичный метод автоматом ухо
    {
        private static List<HubCallerContext> playersInQueue = new List<HubCallerContext>(); // мб заменить на ConcurrentQueue
        private static ConcurrentDictionary<HubCallerContext, Game> playersInGame = new ConcurrentDictionary<HubCallerContext, Game>();

        //private static List<Game> activeGames = new List<Game>(); // когда будет несколько игр, сюда буду их складывать
        // понять, надо ли оно вообще, если у меня есть playersInGame

        private readonly ILogger<GameHub> logger; // всё пишем в логи ради азура
        private readonly IHubContext<GameHub> hubContext; // ради передачи времени
        private readonly DBContext dbContext; // SQL
        private readonly IConfiguration configuration; // ради JWT чтобы не хардкодить ключ

        public GameHub(ILogger<GameHub> logger, IHubContext<GameHub> hubContext, DBContext dBContext, IConfiguration configuration)
        {
            this.logger = logger;
            this.hubContext = hubContext;
            this.dbContext = dBContext;
            this.configuration = configuration;
        }

        public override async Task OnConnectedAsync()
            // если тип будет играть сам против себя? проверить такое несовпадение !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            string id = Context.ConnectionId;

            string username = Context.User.Identity.Name;
            logger.LogInformation($"> {username} вошёл в игру");

            playersInQueue.Add(Context);

            if (playersInQueue.Count >= 2)
                await this.CreateGame();

            await base.OnConnectedAsync();
        }

        public async Task CreateGame()
        {
            HubCallerContext first = playersInQueue.First();
            playersInQueue.Remove(first);
            HubCallerContext second = playersInQueue.First();
            playersInQueue.Remove(second);

            Game game = new Game(first, second, hubContext);

            //activeGames.Add(game);

            playersInGame.TryAdd(first, game);
            playersInGame.TryAdd(second, game);

            await Groups.AddToGroupAsync(game.X.ConnectionId, $"{game.X.User?.Identity?.Name}{game.O.User?.Identity?.Name}");
            await Groups.AddToGroupAsync(game.O.ConnectionId, $"{game.X.User?.Identity?.Name}{game.O.User?.Identity?.Name}");

            game.time.Start();

            await Clients.Client(game.X.ConnectionId).SendAsync("CreateGame", true); // отправляется выполнение MultiplayerGame с Х/О
            await Clients.Client(game.O.ConnectionId).SendAsync("CreateGame", false);
            // try-catch

            logger.LogInformation($"> Создана игра: {game.X.User?.Identity?.Name} / {game.O.User?.Identity?.Name}");
        }

        public async Task Move(int row, int column)
        {
            HubCallerContext player = Context; // отправитель

            Game game = null;
            playersInGame.TryGetValue(player, out game);
            if (game == null)
                throw new Exception("Игрок, отправивший ход, не играет в данный момент");

            char result;
            lock (game)
            {
                result = game.Move(player, row, column);
            }
            if (result == '-') // до игрока даже не дойдёт эта команда, она остановится на сервере
                // мб то, что ниже, даже не важно, потому что такие коды обрабатываются в бездну на стороне клиента
                return;
            else
            {
                int bigFieldPos = game.BigFieldPos(row, column);
                MoveInfo data = new MoveInfo(row, column, game.field[row, column], game.nextMove, result, bigFieldPos, game.bigField[bigFieldPos / 10, bigFieldPos % 10]);
                await Clients.Group($"{game.X.User?.Identity?.Name}{game.O.User?.Identity?.Name}").SendAsync("Move", data);

                logger.LogInformation($"> В игру {game.X.User?.Identity?.Name} / {game.O.User?.Identity?.Name} отправлен корректный ход {row},{column}");

                if (result != '.')
                {
                    await WriteStats(game, result);

                    DisconnectedAction action = DisconnectedAction.Normal;
                    await Clients.Group($"{game.X.User?.Identity?.Name}{game.O.User?.Identity?.Name}").SendAsync("EndGame", action, result);
                    await EndGame(game);

                    logger.LogInformation($"> Игра {game.X.User?.Identity?.Name} / {game.O.User?.Identity?.Name} завершена, игроки отключены");
                }
            }
        }

        public async Task EndGame(Game game) // убрать их из своего списка игроков
        {
            playersInGame.TryRemove(game.X, out _);
            playersInGame.TryRemove(game.O, out _);

            await Groups.RemoveFromGroupAsync(game.X.ConnectionId, $"{game.X.User?.Identity?.Name}{game.O.User?.Identity?.Name}");
            await Groups.RemoveFromGroupAsync(game.O.ConnectionId, $"{game.X.User?.Identity?.Name}{game.O.User?.Identity?.Name}");
        }

        public async override Task OnDisconnectedAsync(Exception? exception) // сделать окно закрытия игры, когда типа просто отключило от сервера
        {
            logger.LogInformation($"> Разорвано подключение: {Context.User.Identity.Name}");

            Game game = null;
            if (playersInQueue.Contains(Context))
            {
                playersInQueue.Remove(Context);
            }
            else if (playersInGame.TryGetValue(Context, out game)) // при окончании игры я удаляю в EndGame игру. если тип отключился сам, игра останется, второму придёт завершение
            {
                char result = 'D';
                if (Context == game.X)
                    result = 'O';
                else
                    result = 'X';

                DisconnectedAction action = DisconnectedAction.Disconnect;
                await WriteStats(game, result);
                await Clients.Group($"{game.X.User?.Identity?.Name}{game.O.User?.Identity?.Name}").SendAsync("EndGame", action, 'D'); // ОТПРАВИТЬ ЕНАМ, ЧТО ИГРА БЫЛА ЗАВЕРШЕНА ВЫХОДОМ СОПЕРНИКА
                await EndGame(game); // мне НЕ нужно полное удаление игры, уже запущен метод дисконнекта

                logger.LogInformation($"> Игра {game.X.User?.Identity?.Name} / {game.O.User?.Identity?.Name} завершена принудительно ({Context.User.Identity.Name} отключился)");
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task WriteStats(Game game, char result)
        {
            var playerX = await dbContext.Users.Include(u => u.stats).FirstOrDefaultAsync(u => u.username == game.X.User.Identity.Name);
            var playerO = await dbContext.Users.Include(u => u.stats).FirstOrDefaultAsync(u => u.username == game.O.User.Identity.Name);

            playerX.stats.games++;
            playerO.stats.games++;

            if (result == 'X')
            {
                playerX.stats.wins++;
                playerO.stats.loses++;
            }
            else if (result == 'O')
            {
                playerO.stats.wins++;
                playerX.stats.loses++;
            }
            else if (result == 'N')
            {
                playerX.stats.draws++;
                playerO.stats.draws++;
            }
            else
            {
                logger.LogInformation($"Произошла ошибка обновления статистики в игре {game.X.User?.Identity?.Name} / {game.O.User?.Identity?.Name} ({result})");
            }

            await dbContext.SaveChangesAsync();
        }
    }
}