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

        /// Summary:
        /// Checks whether two boards have the same dimensions and identical cell positions.
        public bool AreEqual(
            Board first,
            Board second,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);
            cancellationToken.ThrowIfCancellationRequested();

            if (first.Rows != second.Rows || first.Columns != second.Columns)
            {
                return false;
            }

            for (int row = 0; row < first.Rows; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                for (int column = 0; column < first.Columns; column++)
                {
                    if (first.Cells[row, column] != second.Cells[row, column])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
