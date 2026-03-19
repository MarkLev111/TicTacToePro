using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace TicTacToePro
{
    class Game
    {
        public bool XO { get; set; } // true - X, false — O
        public bool[,] field { get; set; }
        public bool[,] bigField { get; set; }
        private int fieldsClosed { get; set; }
        public int nextMove { get; set; }

        public Game()
        {
            this.XO = true;
            this.field = new bool[9, 9];
            this.bigField = new bool[3, 3];
            this.fieldsClosed = 0;
            this.nextMove = -1;
        }

        public void MakeMove(int row, int column)
        {
            int bigFieldPos = BigFieldPos(row, column); // то, в какую большую клетку поставили только что

            if (this.nextMove != bigFieldPos)
                return;

            if (field[row, column] != null) // маленькое поле занято
                return;

            if (bigField[bigFieldPos / 10, bigFieldPos % 10] != null) // большое поле закрыто
                return;

            field[row, column] = XO;

            bool smallField = CheckWinField(row, column);
            if (smallField == true)
            {
                this.fieldsClosed++;
                bigField[bigFieldPos / 10, bigFieldPos % 10] = XO;
                bool gameResult = CheckWinGame();

                // ПРОДУМАТЬ ОКОНЧАНИЕ ИГРЫ
            }

            XO = !XO;
            this.nextMove = NextMovePos(row * 10 + column);
        }

        public bool CheckWinField(int row, int column)
        {
            switch (row)
            {
                case 0: case 3: case 6:
                    switch (column)
                    {
                        case 0: case 3: case 6:
                            if (field[row, column] == field[row, column + 1] == field[row, column + 2]) return true;
                            else if (field[row, column] == field[row + 1, column] == field[row + 2, column]) return true;
                            else if (field[row, column] == field[row + 1, column + 1] == field[row + 2, column + 2]) return true;
                            else return false;
                        case 1: case 4: case 7:
                            if (field[row, column - 1] == field[row, column] == field[row, column + 1]) return true;
                            else if (field[row, column] == field[row + 1, column] == field[row + 2, column]) return true;
                            else return false;
                        case 2: case 5: case 8:
                            if (field[row, column - 2] == field[row, column - 1] == field[row, column]) return true;
                            else if (field[row, column] == field[row + 1, column] == field[row + 2, column]) return true;
                            else if (field[row, column] == field[row + 1, column - 1] == field[row + 2, column - 2]) return true;
                            else return false;
                        default:
                            throw new ArgumentOutOfRangeException($"Поля со столбцом {column} нет!");
                    }
                case 1: case 4: case 7:
                    switch (column)
                    {
                        case 0: case 3: case 6:
                            if (field[row, column] == field[row, column + 1] == field[row, column + 2]) return true;
                            else if (field[row - 1, column] == field[row, column] == field[row + 1, column]) return true;
                            else return false;
                        case 1: case 4: case 7:
                            if (field[row, column - 1] == field[row, column] == field[row, column + 1]) return true;
                            else if (field[row - 1, column] == field[row, column] == field[row + 1, column]) return true;
                            else if (field[row - 1, column - 1] == field[row, column] == field[row + 1, column + 1]) return true;
                            else if (field[row + 1, column - 1] == field[row, column] == field[row - 1, column + 1]) return true;
                            else return false;
                        case 2: case 5: case 8:
                            if (field[row, column - 2] == field[row, column - 1] == field[row, column]) return true;
                            else if (field[row - 1, column] == field[row, column] == field[row + 1, column]) return true;
                            else return false;
                        default:
                            throw new ArgumentOutOfRangeException($"Поля со столбцом {column} нет!");
                    }
                case 2: case 5: case 8:
                    switch (column)
                    {
                        case 0: case 3: case 6:
                            if (field[row, column] == field[row, column + 1] == field[row, column + 2]) return true;
                            else if (field[row, column] == field[row - 1, column] == field[row - 2, column]) return true;
                            else if (field[row, column] == field[row - 1, column + 1] == field[row - 2, column + 2]) return true;
                            else return false;
                        case 1: case 4: case 7:
                            if (field[row, column - 1] == field[row, column] == field[row, column + 1]) return true;
                            else if (field[row, column] == field[row - 1, column] == field[row - 2, column]) return true;
                            else return false;
                        case 2: case 5: case 8:
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

        public bool CheckWinGame()
        {
            return (bigField[0, 0] == bigField[0, 1] == bigField[0, 2]) ||
                   (bigField[1, 0] == bigField[1, 1] == bigField[1, 2]) ||
                   (bigField[2, 0] == bigField[2, 1] == bigField[2, 2]) ||
                   (bigField[0, 0] == bigField[1, 1] == bigField[2, 2]) ||
                   (bigField[2, 0] == bigField[1, 1] == bigField[0, 2]) ||
                   (bigField[0, 0] == bigField[1, 0] == bigField[2, 0]) ||
                   (bigField[0, 1] == bigField[1, 1] == bigField[2, 1]) ||
                   (bigField[0, 2] == bigField[1, 2] == bigField[2, 2]);
        }

        public int BigFieldPos(int row, int column) // выдаёт номер большой клетки из bigField, в которой сделан ход
        {
            switch (row)
            {
                case 0: case 1: case 2:
                    switch (column)
                    {
                        case 0: case 1: case 2:
                            return 0; 
                        case 3: case 4: case 5:
                            return 1;
                        case 6: case 7: case 8:
                            return 2;
                        default:
                            throw new ArgumentOutOfRangeException($"Большой клетки со столбцом {column} нет!");
                    }
                case 3: case 4: case 5:
                    switch (column)
                    {
                        case 0: case 1: case 2:
                            return 10;
                        case 3: case 4: case 5:
                            return 11;
                        case 6: case 7: case 8:
                            return 12;
                        default:
                            throw new ArgumentOutOfRangeException($"Большой клетки со столбцом {column} нет!");
                    }
                case 6: case 7: case 8:
                    switch (column)
                    {
                        case 0: case 1: case 2:
                            return 20;
                        case 3: case 4: case 5:
                            return 21;
                        case 6: case 7: case 8:
                            return 22;
                        default:
                            throw new ArgumentOutOfRangeException($"Большой клетки со столбцом {column} нет!");
                    }
                default:
                    throw new ArgumentOutOfRangeException($"Большой клетки со строкой {row} нет!");
            }
        }

        public int NextMovePos(int bigPos) // выдаёт номер большой клетки из bigField, в которой должен быть следующий ход
        {
            switch (bigPos)
            {
                case 0: case 3: case 6:
                case 30: case 33: case 36:
                case 60: case 63: case 66:
                    return 0;
                case 1: case 4: case 7:
                case 31: case 34: case 37:
                case 61: case 64: case 67:
                    return 1;
                case 2: case 5: case 8:
                case 32: case 35: case 38:
                case 62: case 65: case 68:
                    return 2;

                case 10: case 13: case 16:
                case 40: case 43: case 46:
                case 70: case 73: case 76:
                    return 10;
                case 11: case 14: case 17:
                case 41: case 44: case 47:
                case 71: case 74: case 77:
                    return 11;
                case 12: case 15: case 18:
                case 42: case 45: case 48:
                case 72: case 75: case 78:
                    return 12;

                case 20: case 23: case 26:
                case 50: case 53: case 56:
                case 80: case 83: case 86:
                    return 20;
                case 21: case 24: case 27:
                case 51: case 54: case 57:
                case 81: case 84: case 87:
                    return 21;
                case 22: case 25: case 28:
                case 52: case 55: case 58:
                case 82: case 85: case 88:
                    return 22;
                default:
                    throw new ArgumentOutOfRangeException($"Такого поля {bigPos / 10},{bigPos % 10}, в которое был совершён ход, не существует!");
            }
        }
    }
}
