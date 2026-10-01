using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.Service
{
    public class GameService
    {
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
        public Board NextIterationStep(Board currentBoard)
        {
            ArgumentNullException.ThrowIfNull(currentBoard);
            var nextBoard = new Board(currentBoard.Rows, currentBoard.Columns);

            for (int row = 0; row < currentBoard.Rows; row++)
            {
                for (int column = 0; column < currentBoard.Columns; column++)
                {
                    int neighbours = CountLivingNeighbours(currentBoard, row, column);
                    bool isAlive = _boardService.IsAlive(currentBoard, row, column);
                    bool willBeAlive = neighbours == 3 || (isAlive && neighbours == 2);

                    _boardService.SetCell(nextBoard, row, column, willBeAlive);
                }
            }

            return nextBoard;
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
