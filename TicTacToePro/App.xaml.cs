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
        private static readonly Task<GitHub> task = Authorize.GetLatestVersion();
        public static readonly GitHub latestVersion = task.Result;
    }

}
