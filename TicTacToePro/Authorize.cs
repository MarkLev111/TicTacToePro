using CredentialManagement;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Threading;
using TicTacToePro.Shared;

namespace TicTacToePro
{
    internal class Authorize
    {
        public static string name = "TicTacToePro";
        public static string username { get; set; } = null; // придумать реализацию
        public void SaveToken(string token)
        {
            var cred = new Credential();

            cred.Password = token;
            cred.Target = name;
            cred.Type = CredentialType.Generic;     // Тип для обычных токенов/паролей
            cred.PersistanceType = PersistanceType.LocalComputer; // Хранить после перезагрузки

            cred.Save();
        }

        public static string GetToken()
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

        public static void DeleteToken()
        {
            var cred = new Credential();
            cred.Target = name;
            cred.Delete();
        }

        public static HubConnection Connection()
        {
            HubConnectionBuilder connectionBuilder = new HubConnectionBuilder();
            //connectionBuilder.WithUrl("http://localhost:5195/gamehub", options =>
            //{
            //    options.AccessTokenProvider = () => Task.FromResult(GetToken());
            //}); // сервер локалхост
            connectionBuilder.WithUrl("https://tictactoepro-a6egbyh8ake9cgdv.israelcentral-01.azurewebsites.net/gamehub"); // сервер азур
            connectionBuilder.WithAutomaticReconnect();
            HubConnection connection = connectionBuilder.Build();
            return connection;
        }

        public static async Task LoginRegister(UserData data, Window window)
        {
            HubConnection connection = Authorize.Connection();

            connection.On<Credential>("LoginRegister", async (cred) =>
            {
                if (cred != null)
                {
                    cred.Save();
                    username = data.username;
                }
                await connection.StopAsync();
                window.Close();
            });

            await StartAsync(connection, window);

            await connection.SendAsync("LoginRegister", data);
        }

        public static async Task StartAsync(HubConnection connection, Window window)
        {
            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                // сделать так, чтобы при неудачном подключении к серверу выбрасывалась ошибка и главное меню
                MessageBox.Show("Не удалось подключиться к серверу.");
                window.Close();
            }
        }
    }
}
