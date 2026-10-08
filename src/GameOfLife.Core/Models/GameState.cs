namespace GameOfLife.Core.Models
{
    public class GameState
    {
        public Board Board { get; }
        public long Iteration { get; }

        /// Summary:
        /// Groups the current board and iteration number, starting new games at zero.
        public GameState(Board board, long iteration = 0)
        {
            ArgumentNullException.ThrowIfNull(board);
            ArgumentOutOfRangeException.ThrowIfNegative(iteration);
            Board = board;
            Iteration = iteration;
        }
    }
}
