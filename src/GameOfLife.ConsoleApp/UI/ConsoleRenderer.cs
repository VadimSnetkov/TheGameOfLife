using System.Text;
using GameOfLife.ConsoleApp.Models;
using GameOfLife.ConsoleApp.Service;

namespace GameOfLife.ConsoleApp.UI
{
    public class ConsoleRenderer
    {
        private readonly BoardService _boardService;

        /// Summary:
        /// Receives the service used to read the board for rendering.
        public ConsoleRenderer(BoardService boardService)
        {
            ArgumentNullException.ThrowIfNull(boardService);
            _boardService = boardService;
        }

        /// Summary:
        /// Builds and displays one complete frame of the game field.
        public void Render(Board board)
        {
            ArgumentNullException.ThrowIfNull(board);
            var output = new StringBuilder();
            output.AppendLine("TheGameOfLife");
            output.AppendLine("# = alive, . = dead | Ctrl+C to exit");
            output.AppendLine();

            for (int row = 0; row < board.Rows; row++)
            {
                for (int column = 0; column < board.Columns; column++)
                {
                    output.Append(_boardService.IsAlive(board, row, column) ? "# " : ". ");
                }
                output.AppendLine();
            }

            Console.Clear();
            Console.Write(output.ToString());
        }
    }
}
