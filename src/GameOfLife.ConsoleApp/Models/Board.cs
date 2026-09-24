using System;

namespace GameOfLife.ConsoleApp.Models
{
    public class Board
    {
        private readonly bool[,] _cells;
        public int Rows { get; }
        public int Columns { get; }
        public Board(int rows, int columns)
        {
            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be greater than zero.");
            }
            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be greater than zero.");
            }
            Rows = rows;
            Columns = columns;
            _cells = new bool[rows, columns];
        }

        public bool IsAlive(int row, int column)
        {
            ValidateCoordinates(row, column);
            return _cells[row, column];
        }

        public void SetCell(int row, int column, bool isAlive)
        {
            ValidateCoordinates(row, column);
            _cells[row, column] = isAlive;
        }

        private void ValidateCoordinates(int row, int column)
        {
            if (row < 0 || row >= Rows)
            {
                throw new ArgumentOutOfRangeException(nameof(row), "Row is out of bounds.");
            }
            if (column < 0 || column >= Columns)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "Column is out of bounds.");
            }
        }
    }
}
