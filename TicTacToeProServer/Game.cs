using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

// СЕРВЕРНАЯ ЛОГИКА

namespace TicTacToeProServer
{
    class Game // МБ ПОМЕНЯТЬ ПАБЛИКИ НА ПРАЙВАТЫ
    {
        public HubCallerContext X { get; }
        public HubCallerContext O { get; }
        public bool XO { get; set; } // true - X, false — O
        public char[,] field { get; set; }
        public char[,] bigField { get; set; }
        private int fieldsClosed { get; set; }
        public int nextMove { get; set; }

        public System.Timers.Timer time = new System.Timers.Timer(1000);
        private int seconds = 0;

        private IHubContext<GameHub> hub;

        public Game(HubCallerContext first, HubCallerContext second, IHubContext<GameHub> hub)
        {
            this.XO = true;
            this.field = new char[9, 9];
            this.bigField = new char[3, 3];
            this.fieldsClosed = 0;
            this.nextMove = -1; // -1 = в любое
            this.hub = hub;
            if (new Random().Next(0, 2) == 0) // зарандомили, кто есть кто
            {
                this.X = first;
                this.O = second;
            }
            else
            {
                this.X = second;
                this.O = first;
            }
            time.AutoReset = true;
            time.Elapsed += OnTimerTick;
        }

        public char Move(HubCallerContext player, int row, int column)
        {
            if (!CheckMove(player))
                return '-';

            int bigFieldPos = BigFieldPos(row, column); // то, в какую большую клетку поставили только что

            if (this.nextMove != -1 && this.nextMove != bigFieldPos) // неправильная клетка -
                return '-';

            if (this.field[row, column] != '\0') // маленькое поле занято -
                return '-';

            if (this.bigField[bigFieldPos / 10, bigFieldPos % 10] != '\0') // большое поле закрыто -
                return '-';

            MakeAMove(row, column);

            bool smallField = CheckWinField(row, column);
            if (smallField)
            {
                this.fieldsClosed++;
                this.bigField[bigFieldPos / 10, bigFieldPos % 10] = field[row, column]; // тот же char
                bool gameResult = CheckWinGame();

                if (gameResult) // есть победитель игры
                    return this.bigField[bigFieldPos / 10, bigFieldPos % 10];
            }
            if (CheckFieldClosed(bigFieldPos))
                this.fieldsClosed++;

            if (this.fieldsClosed == 9) // ничья
                return 'N';
            // если закрыты все 9 клеток, а проверка на победителя сверху не прошла,
            // это автоматически ничья.

            XO = !XO;

            NextMove(row, column, bigField[bigFieldPos / 10, bigFieldPos % 10] != '\0', bigFieldPos);
            return '.'; // ИГРАЕМ ДАЛЬШЕ
        }

        public bool CheckMove(HubCallerContext player) // проверка законности хода
        {
            if (player == this.X && XO)
                return true;
            else if (player == this.O && !XO)
                return true;
            else
                return false;
        }

        public bool CheckWinField(int row, int column) // проверяет, что поле не пустое + что символы Х/О равны
        {
            ArgumentOutOfRangeException argcol = new ArgumentOutOfRangeException($"Поля со столбцом {column} нет!");
            switch (row)
            {
                case 0: case 3: case 6:
                    switch (column)
                    {
                        case 0: case 3: case 6:
                            if (this.field[row, column + 1] == this.field[row, column] && this.field[row, column + 2] == this.field[row, column]) return true;
                            else if (this.field[row + 1, column] == this.field[row, column] && this.field[row + 2, column] == this.field[row, column]) return true;
                            else if (this.field[row + 1, column + 1] == this.field[row, column] && this.field[row + 2, column + 2] == this.field[row, column]) return true;
                            else return false;
                        case 1: case 4: case 7:
                            if (this.field[row, column] == this.field[row, column - 1] && this.field[row, column + 1] == this.field[row, column]) return true;
                            else if (this.field[row + 1, column] == this.field[row, column] && this.field[row + 2, column] == this.field[row, column]) return true;
                            else return false;
                        case 2: case 5: case 8:
                            if (this.field[row, column - 2] == this.field[row, column] && this.field[row, column - 1] == this.field[row, column]) return true;
                            else if (this.field[row + 1, column] == this.field[row, column] && this.field[row + 2, column] == this.field[row, column]) return true;
                            else if (this.field[row + 1, column - 1] == this.field[row, column] && this.field[row + 2, column - 2] == this.field[row, column]) return true;
                            else return false;
                        default:
                            throw argcol;
                    }
                case 1: case 4: case 7:
                    switch (column)
                    {
                        case 0: case 3: case 6:
                            if (this.field[row, column + 1] == this.field[row, column] && this.field[row, column + 2] == this.field[row, column]) return true;
                            else if (this.field[row, column] == this.field[row - 1, column] && this.field[row + 1, column] == this.field[row, column]) return true;
                            else return false;
                        case 1: case 4: case 7:
                            if (this.field[row, column] == this.field[row, column - 1] && this.field[row, column + 1] == this.field[row, column]) return true;
                            else if (this.field[row - 1, column] == this.field[row, column] && this.field[row + 1, column] == this.field[row, column]) return true;
                            else if (this.field[row - 1, column - 1] == this.field[row, column] && this.field[row + 1, column + 1] == this.field[row, column]) return true;
                            else if (this.field[row + 1, column - 1] == this.field[row, column] && this.field[row - 1, column + 1] == this.field[row, column]) return true;
                            else return false;
                        case 2: case 5: case 8:
                            if (this.field[row, column - 1] == this.field[row, column] && this.field[row, column - 2] == this.field[row, column]) return true;
                            else if (this.field[row, column] == this.field[row - 1, column] && this.field[row + 1, column] == this.field[row, column]) return true;
                            else return false;
                        default:
                            throw argcol;
                    }
                case 2: case 5: case 8:
                    switch (column)
                    {
                        case 0: case 3: case 6:
                            if (this.field[row, column + 1] == this.field[row, column] && this.field[row, column + 2] == this.field[row, column]) return true;
                            else if (this.field[row - 1, column] == this.field[row, column] && this.field[row - 2, column] == this.field[row, column]) return true;
                            else if (this.field[row - 1, column + 1] == this.field[row, column] && this.field[row - 2, column + 2] == this.field[row, column]) return true;
                            else return false;
                        case 1: case 4: case 7:
                            if (this.field[row, column] == this.field[row, column - 1] && this.field[row, column + 1] == this.field[row, column]) return true;
                            else if (this.field[row - 2, column] == this.field[row, column] && this.field[row - 1, column] == this.field[row, column]) return true;
                            else return false;
                        case 2: case 5: case 8:
                            if (this.field[row, column - 1] == this.field[row, column] && this.field[row, column] == this.field[row, column - 2]) return true;
                            else if (this.field[row - 1, column] == this.field[row, column] && this.field[row - 2, column] == this.field[row, column]) return true;
                            else if (this.field[row - 1, column - 1] == this.field[row, column] && this.field[row - 2, column - 2] == this.field[row, column]) return true;
                            else return false;
                        default:
                            throw argcol;
                    }
                default:
                    throw new ArgumentOutOfRangeException($"Поля с рядом {row} нет!");
            }
        }

        // если нет МЕСТА для фигуры, то ниже
        public bool CheckFieldClosed(int bigFieldPos) // если в большой клетке получилась ничья (ранее победа дала false)
        {
            int row = 0;
            int column = 0;
            switch(bigFieldPos)
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

        //public bool CheckClosed()
        //{
        //    for (int i = 0; i < 3; i++)
        //        for (int j = 0; j < 3; j++)
        //            if (bigField[i, j] == '\0')
        //                return false;
        //    return true;
        //}

        public bool CheckWinGame() // все варианты закончить игру чёткой победой
        {
            return (this.bigField[0, 0] != '\0' && this.bigField[0, 0] == this.bigField[0, 1] && this.bigField[0, 1] == this.bigField[0, 2]) ||
                   (this.bigField[1, 0] != '\0' && this.bigField[1, 0] == this.bigField[1, 1] && this.bigField[1, 1] == this.bigField[1, 2]) ||
                   (this.bigField[2, 0] != '\0' && this.bigField[2, 0] == this.bigField[2, 1] && this.bigField[2, 1] == this.bigField[2, 2]) ||
                   (this.bigField[0, 0] != '\0' && this.bigField[0, 0] == this.bigField[1, 1] && this.bigField[1, 1] == this.bigField[2, 2]) ||
                   (this.bigField[2, 0] != '\0' && this.bigField[2, 0] == this.bigField[1, 1] && this.bigField[1, 1] == this.bigField[0, 2]) ||
                   (this.bigField[0, 0] != '\0' && this.bigField[0, 0] == this.bigField[1, 0] && this.bigField[1, 0] == this.bigField[2, 0]) ||
                   (this.bigField[0, 1] != '\0' && this.bigField[0, 1] == this.bigField[1, 1] && this.bigField[1, 1] == this.bigField[2, 1]) ||
                   (this.bigField[0, 2] != '\0' && this.bigField[0, 2] == this.bigField[1, 2] && this.bigField[1, 2] == this.bigField[2, 2]);
        }

        public int BigFieldPos(int row, int column) // выдаёт номер большой клетки из bigField, в которой сделан ход
        {
            ArgumentOutOfRangeException argcol = new ArgumentOutOfRangeException($"Большой клетки со столбцом {column} нет!");
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
                            throw argcol;
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
                            throw argcol;
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
                            throw argcol;
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

        public void NextMove(int row, int column, bool fieldClosed, int bigFieldPos)
        {
            int nextMoveSupposed = NextMovePos(row * 10 + column);
            if (bigField[nextMoveSupposed / 10, nextMoveSupposed % 10] == '\0' && !CheckFieldClosed(nextMoveSupposed))
            {
                this.nextMove = nextMoveSupposed;
                return;
            }
            if (!CheckFieldClosed(this.nextMove) && !fieldClosed)
                if (this.nextMove == -1)
                    this.nextMove = bigFieldPos;
                else
                    return;
            else
                this.nextMove = -1;
        }

        public void MakeAMove(int row, int column)
        {
            switch (XO)
            {
                case true:
                    this.field[row, column] = 'X';
                    return;
                case false:
                    this.field[row, column] = 'O';
                    return;
            }
        }

        private async void OnTimerTick(System.Object? o, ElapsedEventArgs? e)
        {
            this.seconds++;
            await hub.Clients.Group($"{X}{O}").SendAsync("Timer", seconds);
        }

        public int GetSeconds()
        {
            return this.seconds;
        }
    }
}
