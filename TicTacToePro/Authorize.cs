using System;
using System.Collections.Generic;
using System.Text;
using CredentialManagement;

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

        public void DeleteToken()
        {
            var cred = new Credential();
            cred.Target = name;
            cred.Delete();
        }


    }
}
