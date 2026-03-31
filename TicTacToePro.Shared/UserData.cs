namespace TicTacToePro.Shared
{
    public class UserData
    {
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public UserData(string email, string username, string password)
        {
            this.email = email;
            this.username = username;
            this.password = password;
        }

        public UserData() { }
    }
}
