using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.Service
{
    public class BoardService
    {
        /// Summary:
        /// Returns whether the cell at the specified coordinates is alive.
        public bool IsAlive(Board board, int row, int column)
        {
            ValidateCoordinates(board, row, column);
            return board.Cells[row, column];
        }

        /// Summary:
        /// Updates one cell after checking that its coordinates are inside the board.
        public void SetCell(Board board, int row, int column, bool isAlive)
        {
            ValidateCoordinates(board, row, column);
            board.Cells[row, column] = isAlive;
        }

        /// Summary:
        /// Uses the .NET guard methods to reject a missing board or invalid coordinates.
        private void ValidateCoordinates(Board board, int row, int column)
        {
            ArgumentNullException.ThrowIfNull(board);
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, board.Rows);
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, board.Columns);
        }
    }
}
