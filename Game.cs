using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToePro
{
    class Game
    {
        public bool XO { get; set; } // true - X, false — O
        public bool[,] field { get; set; }
        public bool[] bigField { get; set; }
        public int nextMove { get; set; }

        public Game()
        {
            this.XO = true;
            this.field = new bool[9,9];
            this.bigField = new bool[9];
            this.nextMove = -1;
        }

        public void MakeMove(int row, int column)
        {
            if (Math.Abs(nextMove - row) > 2) // nextMove будет в диапазоне 3 чисел
                return;

            if (field[row, column] != null)
                return;

            field[row, column] = XO;
            XO = !XO;
        }

        public bool CheckWinField(int row, int column)
        {
            switch (row)
            {
                case 0:
                case 3:
                case 6:
                    switch (column)
                    {
                        case 0:
                        case 3:
                        case 6:
                            if (field[row, column] == field[row, column + 1] == field[row, column + 2]) return true;
                            else if (field[row, column] == field[row + 1, column] == field[row + 2, column]) return true;
                            else if (field[row, column] == field[row + 1, column + 1] == field[row + 2, column + 2]) return true;
                            else return false;
                        case 1:
                        case 4:
                        case 7:
                            if (field[row, column - 1] == field[row, column] == field[row, column + 1]) return true;
                            else if (field[row, column] == field[row + 1, column] == field[row + 2, column]) return true;
                            else return false;
                        case 2:
                        case 5:
                        case 8:
                            if (field[row, column - 2] == field[row, column - 1] == field[row, column]) return true;
                            else if (field[row, column] == field[row + 1, column] == field[row + 2, column]) return true;
                            else if (field[row, column] == field[row + 1, column - 1] == field[row + 2, column - 2]) return true;
                            else return false;
                        default:
                            throw new ArgumentOutOfRangeException($"Поля со столбцом {column} нет!");
                    }
                case 1:
                case 4:
                case 7:
                    switch (column)
                    {
                        case 0:
                        case 3:
                        case 6:
                            if (field[row, column] == field[row, column + 1] == field[row, column + 2]) return true;
                            else if (field[row - 1, column] == field[row, column] == field[row + 1, column]) return true;
                            else return false;
                        case 1:
                        case 4:
                        case 7:
                            if (field[row, column - 1] == field[row, column] == field[row, column + 1]) return true;
                            else if (field[row - 1, column] == field[row, column] == field[row + 1, column]) return true;
                            else if (field[row - 1, column - 1] == field[row, column] == field[row + 1, column + 1]) return true;
                            else if (field[row + 1, column - 1] == field[row, column] == field[row - 1, column + 1]) return true;
                            else return false;
                        case 2:
                        case 5:
                        case 8:
                            if (field[row, column - 2] == field[row, column - 1] == field[row, column]) return true;
                            else if (field[row - 1, column] == field[row, column] == field[row + 1, column]) return true;
                            else return false;
                        default:
                            throw new ArgumentOutOfRangeException($"Поля со столбцом {column} нет!");
                    }
                case 2:
                case 5:
                case 8:
                    switch (column)
                    {
                        case 0:
                        case 3:
                        case 6:
                            if (field[row, column] == field[row, column + 1] == field[row, column + 2]) return true;
                            else if (field[row, column] == field[row - 1, column] == field[row - 2, column]) return true;
                            else if (field[row, column] == field[row - 1, column + 1] == field[row - 2, column + 2]) return true;
                            else return false;
                        case 1:
                        case 4:
                        case 7:
                            if (field[row, column - 1] == field[row, column] == field[row, column + 1]) return true;
                            else if (field[row, column] == field[row - 1, column] == field[row - 2, column]) return true;
                            else return false;
                        case 2:
                        case 5:
                        case 8:
                            if (field[row, column - 2] == field[row, column - 1] == field[row, column]) return true;
                            else if (field[row, column] == field[row - 1, column] == field[row - 2, column]) return true;
                            else if (field[row, column] == field[row - 1, column - 1] == field[row - 2, column - 2]) return true;
                            else return false;
                        default:
                            throw new ArgumentOutOfRangeException($"Поля со столбцом {column} нет!");
                    }
                default:
                    throw new ArgumentOutOfRangeException($"Поля с рядом {row} нет!");
            }
        }
    }
}
