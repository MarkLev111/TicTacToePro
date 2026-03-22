using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToePro
{
    internal class MultiplayerGame : Game
    {
        //public bool XO { get; }
        //public char[,] field { get; }
        //public char[,] bigField { get; }
        //public int nextMove { get; set; }
        public bool myTurn { get; }

        public MultiplayerGame(bool XO)
        {
            this.field = new char[9, 9];
            this.bigField = new char[3, 3];
            this.XO = XO;
            this.myTurn = XO;

            if (XO)
                this.nextMove = -1;
            else
                this.nextMove = -2; // Ход соперника
        }

        // нужно, чтобы срабатывали местные массивы
        public override bool CheckFieldClosed(int bigFieldPos) // если в большой клетке получилась ничья (ранее победа дала false)
        {
            int row = 0;
            int column = 0;
            switch (bigFieldPos)
            {
                case 1: column += 3; goto default;
                case 2: column += 6; goto default;
                case 10: row += 3; goto default;
                case 11: row += 3; column += 3; goto default;
                case 12: row += 3; column += 6; goto default;
                case 20: row += 6; goto default;
                case 21: row += 6; column += 3; goto default;
                case 22: row += 6; column += 6; goto default;

                case 0:
                default:
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            if (this.field[row + i, column + j] == '\0')
                                return false;
                    return true;
            }
        }

        public override bool GetMyTurn()
        {
            return this.myTurn;
        }
    }
}
