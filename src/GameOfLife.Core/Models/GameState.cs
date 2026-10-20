namespace GameOfLife.Core.Models
{
    public class GameState
    {
        public Board Board { get; }
        public long Iteration { get; }
        public bool IsStagnant { get; }

        /// Summary:
        /// Stores the board, iteration number, and whether living cells stopped changing.
        public GameState(
            Board board,
            long iteration = 0,
            bool isStagnant = false)
        {
            ArgumentNullException.ThrowIfNull(board);
            ArgumentOutOfRangeException.ThrowIfNegative(iteration);

            Board = board;
            Iteration = iteration;
            IsStagnant = isStagnant;
        }
    }
}
