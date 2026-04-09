using CredentialManagement;
using Microsoft.AspNetCore.SignalR.Client;
using System.IdentityModel.Tokens.Jwt;
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
        }

        internal static string GetToken()
        {
            using (var cred = new Credential())
            {
                cred.Target = name;
                if (cred.Load())
                {
                    return cred.Password;
                }
            }
            return null;
        }

        internal static void DeleteToken()
        {
            var cred = new Credential();
            cred.Target = name;
            cred.Delete();
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

        internal static async void MainWindowConnect(MainWindow window)
        {
            window.connection = Authorize.Connection(); // ссылка
            ConnectionOn(window.connection, window); // УШИ КОННЕКТА
            await Connect(window.connection, window);
        }

        internal static async Task LoginRegister(UserData data, Window window) // чисто на логин и регу
        {
            HubConnection connection = Authorize.Connection();

            connection.On<string>("SaveToken", token =>
            {
                Authorize.SaveToken(token);
            });

            connection.On<string>("Error", async (errorMessage) =>
            {
                MessageBox.Show(errorMessage, "TicTacToePro");
                await connection.StopAsync();
            });

            await Connect(connection, window);

            await connection.SendAsync("LoginRegister", data);
        }

        internal static HubConnection Connection()
        {
            HubConnectionBuilder connectionBuilder = new HubConnectionBuilder();
            //connectionBuilder.WithUrl("http://localhost:5195/gamehub", options =>
            //{
            //    options.AccessTokenProvider = () => Task.FromResult(GetToken());
            //}); // сервер локалхост
            connectionBuilder.WithUrl("https://tictactoepro-a6egbyh8ake9cgdv.israelcentral-01.azurewebsites.net/gamehub" + GetToken()); // сервер азур
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
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к серверу.", "TicTacToePro");
                window.Close();
            }
        }
    }
}
