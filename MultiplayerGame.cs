using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToePro
{
    internal class MultiplayerGame
    {
        public char[,] field { get; }
        public char[,] bigField { get; }
        public int nextMove { get; set; }

        public MultiplayerGame()
        {
            this.field = new char[9, 9];
            this.bigField = new char[3, 3];
            this.nextMove = -1; // -1 = в любое
        }
    }
}
