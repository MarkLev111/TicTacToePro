using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TicTacToePro.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace TicTacToeProServer
{
    [Authorize]
    class GameHub : Hub // заимствование класса из сигнала
    // любой публичный метод автоматом ухо
    {
        private static List<string> idsInQueue = new List<string>(); // мб заменить на ConcurrentQueue
        private static ConcurrentDictionary<string, Game> playersInGame = new ConcurrentDictionary<string, Game>();

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

            if (Context.User.Identity.IsAuthenticated)
            {
                string username = Context.User.Identity.Name;
                logger.LogInformation($"> {username} вошёл в игру");

                idsInQueue.Add(id);

                if (idsInQueue.Count >= 2)
                    await this.CreateGame();
            }
            else
            {
                logger.LogInformation($"> Установление анонимное подключение: {id}");
            }

            await base.OnConnectedAsync();
        }

        [AllowAnonymous]
        public async Task Authorize(UserData data) // запрос на логин или регистрацию
        {
            if (data.email == null) // логин
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.username == data.username);
                if (user == null)
                {
                    await Clients.User(Context.ConnectionId).SendAsync("Error", "Такого пользователя не существует.");
                    return;
                }
                else
                {
                    bool passwordCheck = BCrypt.Net.BCrypt.Verify(data.password, user.password);
                    if (!passwordCheck)
                    {
                        await Clients.User(Context.ConnectionId).SendAsync("Error", "Неверный пароль.");
                        return;
                    }
                }
            }
            else // регистрация
            {
                data.password = BCrypt.Net.BCrypt.HashPassword(data.password);

                bool existsEmail = await dbContext.Users.AnyAsync(u => u.email == data.email);
                bool existsUsername = await dbContext.Users.AnyAsync(u => u.username == data.username);
                if (existsEmail && existsUsername)
                {
                    await Clients.User(Context.ConnectionId).SendAsync("Error", "Игрок с такой почтой и таким никнеймом уже существует.");
                    return;
                }
                else if (existsEmail)
                {
                    await Clients.User(Context.ConnectionId).SendAsync("Error", "Игрок с такой почтой уже существует.");
                    return;
                }
                else if (existsUsername)
                {
                    await Clients.User(Context.ConnectionId).SendAsync("Error", "Игрок с таким никнеймом уже существует.");
                    return;
                }
                else
                {
                    dbContext.Users.Add(data);
                    await dbContext.SaveChangesAsync();
                }
            }

            string token = SetToken(data);
            await Clients.User(Context.ConnectionId).SendAsync("SaveToken", token);
        }

        private string SetToken(UserData user)
        {
            var claims = new List<Claim> // общедоступная инфа о токене
            {
                new Claim(ClaimTypes.Name, user.username),
                new Claim(ClaimTypes.Email, user.email),
                new Claim("RegistrationDate", DateTime.UtcNow.ToString()) // а надо ли?
            };

            // 2. Достаем секретный ключ из конфига
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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

            await Clients.Client(game.X).SendAsync("CreateGame", true); // отправляется выполнение MultiplayerGame с Х/О
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