using CredentialManagement;
using Microsoft.AspNetCore.SignalR.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Windows;
using System.Windows.Threading;
using TicTacToePro.Shared;

namespace TicTacToePro
{
    internal class Authorize
    {
        internal static string name = "TicTacToePro";
        internal static string username { get; set; } = GetUsernameFromToken(GetToken());
        internal static readonly HttpClient httpClient = new HttpClient();
        internal static void SaveToken(string token) // когда сервер передаёт токен, вшиваем его в винду с параметрами игры
        {
            using (var cred = new Credential())
            {
                cred.Password = token;
                cred.Target = name;
                cred.Type = CredentialType.Generic;     // Тип для обычных токенов/паролей
                cred.PersistanceType = PersistanceType.LocalComputer; // Хранить после перезагрузки

                cred.Save();
            }
            username = GetUsernameFromToken(token); // обновить на всякий
        }

        internal static string GetToken()
        {
            using (var cred = new Credential())
            {
                cred.Target = name;
                if (cred.Load() && IsTokenValid(cred.Password))
                {
                    return cred.Password;
                }
                DeleteToken();
                return null;
            }
        }

        internal static void DeleteToken()
        {
            using (var cred = new Credential())
            {
                cred.Target = name;
                cred.Delete();
            }
            username = null;
        }

        internal static string GetUsernameFromToken(string token)
        {
            if (token == null)
                return null;
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name");

            return nameClaim?.Value;
        }

        private static bool IsTokenValid(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        internal static async void MainWindowConnect(MainWindow window)
        {
            window.connection = Authorize.Connection(); // ссылка
            ConnectionOn(window.connection, window); // УШИ КОННЕКТА
            await Connect(window.connection, window);
        }

        internal static async Task LoginRegister(UserData data, Window window) // чисто на логин и регу
        {

            var response = await httpClient.PostAsJsonAsync("https://localhost:7224/api/auth/login", data);

            if (response.IsSuccessStatusCode)
            {
                string token = await response.Content.ReadAsStringAsync();
                token = token.Trim('"');
                SaveToken(token);
            }
            else
            {
                MessageBox.Show($"{response.ReasonPhrase}", "TicTacToePro");
            }
        }

        internal static HubConnection Connection()
        {
            HubConnectionBuilder connectionBuilder = new HubConnectionBuilder();

            connectionBuilder.WithUrl("https://localhost:7224/gamehub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(GetToken());
            }); // сервер локалхост

            //connectionBuilder.WithUrl("https://tictactoepro-a6egbyh8ake9cgdv.israelcentral-01.azurewebsites.net/gamehub", options =>
            //{
            //    options.AccessTokenProvider = () => Task.FromResult(GetToken());
            //}); // сервер азур

            connectionBuilder.WithAutomaticReconnect();
            HubConnection connection = connectionBuilder.Build();
            return connection;
        }

        private static async void ConnectionOn(HubConnection connection, MainWindow window)
        {
            // connection.On <ТИП ПРИНИМАЕМЫХ ДАННЫХ ОТ СЕРВЕРА> ("СЕРВЕРНЫЙ МЕТОД", (ПЕРЕМЕННАЯ) =>
            // {
            //      что делать
            // });

            connection.On<MoveInfo>("Move", (data) =>
            {
                window.Dispatcher.Invoke(() => window.Move(data)); // только внутренний код может трогать свой UI
            });

            connection.On<bool>("CreateGame", (XO) => // отправка пакета может быть другой !!!
            {
                window.Dispatcher.Invoke(() =>
                {
                    window.game = new MultiplayerGame(XO);
                    window.WindowTitle();
                    window.UpdateUI(window.game);
                });
            });

            connection.On<DisconnectedAction>("EndGame", async (action) =>
            {
                await connection.StopAsync();
                if (action == DisconnectedAction.Disconnect) // ЭТО ЗНАЧИТ, ЧТО СОПЕРНИК ДИСКОННЕКТНУЛСЯ
                {
                    window.Dispatcher.Invoke(() => window.GameResultWindow('D'));
                }
            });

            connection.On<int>("Timer", (seconds) =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    window.seconds = seconds;
                    window.WindowTitle();
                });
            });

            connection.On<string>("SaveToken", (token) =>
            {
                Authorize.SaveToken(token);
            });
        }

        internal static async Task Connect(HubConnection connection, Window window)
        {
            try
            {
                await connection.StartAsync();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 401
            {
                MessageBox.Show("Вы не авторизованы для игры по сети", "TicTacToePro");
                if (window is MainWindow)
                    window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к серверу.", "TicTacToePro");
                if (window is MainWindow)
                    window.Close();
            }
        }

        internal static bool TokenCheck(Window window)
        {
            return true; // ВРЕМЕННО ДЛЯ РАЗРАБОТКИ
            if (GetToken() == null)
            {
                MessageBox.Show("Вы не авторизованы для игры по сети", "TicTacToePro");
                //window.Close();
                return false;
            }
            return true;
        }
    }
}
