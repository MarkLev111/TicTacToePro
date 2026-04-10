using System.ComponentModel;
using System.Windows;
using TicTacToePro.Shared;

namespace TicTacToePro
{
    public partial class Login : Window
    {
        internal static bool progress { get; set; } = false;
        public Login()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (progress)
                return;
            base.OnClosing(e);
            Menu menu = new Menu();
            menu.Show();
        }

        public async void Login_Click(object sender, EventArgs e) // доделать на запрос обратно от сервера
        {
            if (progress) // чтобы не спамили кнопку
                return;
            progress = true;
            if (this.LoginText.Text == "" || this.PasswordText.Text == "")
            {
                progress = false;
                return;
            }
            UserData data = new UserData(null, this.LoginText.Text, this.PasswordText.Text);
            Authorize.LoginRegister(data, this);
        }

        public void Register_Click(object sender, EventArgs e)
        {
            if (progress)
                return;
            progress = true;
            Register register = new Register();
            register.Show();
            this.Close();
        }
    }
}
