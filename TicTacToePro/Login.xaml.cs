using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR.Client;
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
using TicTacToePro.Shared;

namespace TicTacToePro
{
    public partial class Login : Window
    {
        private Menu menu { get; set; } = new Menu();
        private bool loginInProgress { get; set; } = false;
        public Login()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            menu.Show();
        }

        public async void Login_Click(object sender, EventArgs e)
        {
            if (loginInProgress == true) // чтобы не спамили кнопку
                return;
            loginInProgress = true;
            UserData data = new UserData(null, this.LoginText.Text, this.PasswordText.Text);
            Authorize.LoginRegister(data, this);
        }

        public void Register_Click(object sender, EventArgs e)
        {
            if (loginInProgress == true)
                return;

        }


    }
}
