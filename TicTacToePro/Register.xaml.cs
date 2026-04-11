using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using TicTacToePro.Shared;

namespace TicTacToePro
{
    public partial class Register : Window
    {
        internal static bool registerInProgress { get; set; } = false;
        public Register()
        {
            InitializeComponent();
        }

        public void Register_Click(object sender, EventArgs e)
        {
            if (registerInProgress == true)
                return;
            registerInProgress = true;
            if (this.EmailText.Text == "" || this.LoginText.Text == "" || this.PasswordText.Text == "")
            {
                registerInProgress = false;
                return;
            }
            UserData data = new UserData(this.EmailText.Text, this.LoginText.Text, this.PasswordText.Text, new MultiplayerStats());
            Authorize.LoginRegister(data, this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Menu menu = new Menu();
            menu.Show();
        }

    }
}
