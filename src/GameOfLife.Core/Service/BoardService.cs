using GameOfLife.Core.Models;

namespace GameOfLife.Core.Service
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
        /// Counts the living cells on the supplied board.
        public int CountLivingCells(Board board)
        {
            ArgumentNullException.ThrowIfNull(board);
            int count = 0;
            for (int row = 0; row < board.Rows; row++)
            {
                for (int column = 0; column < board.Columns; column++)
                {
                    if (board.Cells[row, column])
                    {
                        count++;
                    }
                }
            }
            return count;
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
