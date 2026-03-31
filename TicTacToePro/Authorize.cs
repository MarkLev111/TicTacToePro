using System;
using System.Collections.Generic;
using System.Text;
using CredentialManagement;
using Microsoft.AspNetCore.SignalR.Client;

namespace TicTacToePro
{
    internal class Authorize
    {
        public static string name = "TicTacToePro";
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
    }
}
