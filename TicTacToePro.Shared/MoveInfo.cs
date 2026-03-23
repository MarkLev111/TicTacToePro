namespace TicTacToePro.Shared
{
    public enum PostGameAction { None, NewGame, GoToMenu } // на гейм меню
    public enum DisconnectedAction { None, Disconnect, Normal } // на дисконнект от сервера, в первую очередь, неестественный
    public class MoveInfo
    {
        public int row { get; set; }
        public int column { get; set; }
        public char XOToPut { get; set; }
        public int nextMove { get; set; }
        public char result { get; set; }
        public int bigFieldPos { get; set; }
        public char bigFieldChange { get; set; }

        public MoveInfo(int row, int column, char XOToPut, int nextMove, char result, int bigFieldPos, char bigFieldChange)
        {
            this.row = row;
            this.column = column;
            this.XOToPut = XOToPut;
            this.nextMove = nextMove;
            this.result = result;
            this.bigFieldPos = bigFieldPos;
            this.bigFieldChange = bigFieldChange;
        }
        public MoveInfo() { } // гемини сказал поставить конструктор без параметров
    }
}
