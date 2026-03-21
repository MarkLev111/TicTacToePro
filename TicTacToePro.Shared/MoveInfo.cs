namespace TicTacToePro.Shared
{
    public class MoveInfo
    {
        public int row { get; init; }
        public int column { get; init; }
        public char XOToPut { get; init; }
        public int nextMove { get; init; }
        public char result { get; init; }

        public MoveInfo(int row, int column, char XOToPut, int nextMove, char result)
        {
            this.row = row;
            this.column = column;
            this.XOToPut = XOToPut;
            this.nextMove = nextMove;
            this.result = result;
        }
        public MoveInfo() { } // гемини сказал поставить конструктор без параметров
    }
}
