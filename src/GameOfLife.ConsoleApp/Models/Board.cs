namespace GameOfLife.ConsoleApp.Models
{
    public class Board
    {
        internal bool[,] Cells { get; }

        public int Rows { get; }
        public int Columns { get; }

        /// Summary:
        /// Creates an empty board with positive dimensions.
        public Board(int rows, int columns)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

            Rows = rows;
            Columns = columns;
            Cells = new bool[rows, columns];
        }
    }
}
