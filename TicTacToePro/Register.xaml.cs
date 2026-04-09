using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using TicTacToePro.Shared;

namespace TicTacToePro
{
    public partial class Register : Window
    {
        private bool registerInProgress { get; set; } = false;
        public Register()
        {
            InitializeComponent();
        }

        public void Register_Click(object sender, EventArgs e)
        {
            if (registerInProgress == true)
                return;
            registerInProgress = true;
            UserData data = new UserData(this.EmailText.Text, this.LoginText.Text, this.PasswordText.Text);
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
