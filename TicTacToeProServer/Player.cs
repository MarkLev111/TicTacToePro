using Microsoft.AspNetCore.Connections.Features;

namespace TicTacToeProServer
{
    class Player
    {
        public string id { get; }
        public Game game { get; }

        public Player(string id, Game game)
        {
            this.id = id;
            this.game = game;
        }

    }
}
