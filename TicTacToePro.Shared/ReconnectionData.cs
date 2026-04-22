using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToePro.Shared
{
    public class ReconnectionData
    {
        public bool XO { get; set; }
        public char[,] field { get; set; }
        public char[,] bigField { get; set; }
        public int nextMove { get; set; }

        public ReconnectionData(bool XO,  char[,] field, char[,] bigField, int nextMove)
        {
            this.XO = XO;
            this.field = field;
            this.bigField = bigField;
            this.nextMove = nextMove;
        }
        public ReconnectionData() { }
    }
}
