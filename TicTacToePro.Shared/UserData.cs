namespace TicTacToePro.Shared
{
    public class UserData
    {
        public int Id { get; set; }
        public string? email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public MultiplayerStats stats { get; set; }

        public UserData(string email, string username, string password, MultiplayerStats stats)
        {
            this.email = email;
            this.username = username;
            this.password = password;
            this.stats = stats;
        }

        public UserData() { }
    }
}
