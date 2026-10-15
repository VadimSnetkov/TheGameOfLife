using GameOfLife.Core.Models;

namespace GameOfLife.Core.Service
{
    public class GameService
    {
        private const int NeighboursRequiredForBirth = 3;
        private const int MinimumNeighboursForSurvival = 2;
        private const int MaximumNeighboursForSurvival = 3;
        private readonly BoardService _boardService;

        /// Summary:
        /// Receives the service used to read and update board cells.
        public GameService(BoardService boardService)
        {
            ArgumentNullException.ThrowIfNull(boardService);
            _boardService = boardService;
        }

        /// Summary:
        /// Returns the next generation without modifying the supplied board.
        public Board NextIterationStep(Board currentBoard, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(currentBoard);
            cancellationToken.ThrowIfCancellationRequested();
            var nextBoard = new Board(currentBoard.Rows, currentBoard.Columns);

            for (int row = 0; row < currentBoard.Rows; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int column = 0; column < currentBoard.Columns; column++)
                {
                    int neighbours = CountLivingNeighbours(currentBoard, row, column);
                    bool isAlive = _boardService.IsAlive(currentBoard, row, column);
                    bool willBeAlive = isAlive
                        ? neighbours >= MinimumNeighboursForSurvival
                            && neighbours <= MaximumNeighboursForSurvival
                        : neighbours == NeighboursRequiredForBirth;

                    _boardService.SetCell(nextBoard, row, column, willBeAlive);
                }
            }

            return nextBoard;
        }

        /// Summary:
        /// Advances the game and detects whether its living cells have stopped changing.
        public GameState NextIterationStep(
            GameState currentState,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(currentState);

            long nextIteration = checked(currentState.Iteration + 1);

            Board nextBoard = NextIterationStep(
                currentState.Board,
                cancellationToken);

            bool isStagnant =
                _boardService.CountLivingCells(nextBoard) > 0
                && _boardService.AreEqual(
                    currentState.Board,
                    nextBoard,
                    cancellationToken);

            return new GameState(nextBoard, nextIteration, isStagnant);
        }

        /// Summary:
        /// Counts the eight surrounding cells, treating positions outside the field as dead.
        private int CountLivingNeighbours(Board board, int row, int column)
        {
            int count = 0;
            for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
                {
                    if (rowOffset == 0 && columnOffset == 0)
                    {
                        continue;
                    }

                    int neighbourRow = row + rowOffset;
                    int neighbourColumn = column + columnOffset;

                    // This is the finite-field rule, not user-input validation.
                    if (neighbourRow < 0 || neighbourRow >= board.Rows ||
                        neighbourColumn < 0 || neighbourColumn >= board.Columns)
                    {
                        continue;
                    }

                    if (_boardService.IsAlive(board, neighbourRow, neighbourColumn))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
