using System.Configuration;
using System.Data;
using System.Windows;

namespace TicTacToePro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly Version currentVersion = new Version("2.2");
        public static GitHub latestVersion = new GitHub();
        //public static readonly GitHub latestVersion = new GitHub("2.1", "https://github.com/MarkLev111/TicTacToePro/releases/tag/2.1"); // для тестов
    }

}
