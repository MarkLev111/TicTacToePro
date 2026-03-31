using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TicTacToePro
{
    public partial class Login : Window
    {
        private Menu menu { get; set; } = new Menu();
        public Login()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            menu.Show();
        }

        public void Login_Click(object sender, EventArgs e)
        {

        }

        public void Register_Click(object sender, EventArgs e)
        {

        }
    }
}
