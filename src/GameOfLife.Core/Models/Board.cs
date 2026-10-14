namespace GameOfLife.Core.Models
{
    public class Board
    {
        internal bool[,] Cells { get; }

        public int Rows { get; }
        public int Columns { get; }

        /// Summary:
        /// Constructor creates an empty board with positive dimensions.
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
