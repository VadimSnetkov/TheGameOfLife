using System.Text;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;

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
        /// Displays the board, iteration count, living-cell count, controls, and latest save message.
        public void Render(GameState state, string? message = null)
        {
            ArgumentNullException.ThrowIfNull(state);
            Board board = state.Board;
            var output = new StringBuilder();
            output.AppendLine("TheGameOfLife");
            output.AppendLine($"Iteration: {state.Iteration} | Living cells: {_boardService.CountLivingCells(board)}");
            output.AppendLine("# = alive, . = dead | S: save | Q / Esc / Ctrl+C: exit");
            if (message is not null)
            {
                output.AppendLine(message);
            }
            output.AppendLine();

            for (int row = 0; row < board.Rows; row++)
            {
                for (int column = 0; column < board.Columns; column++)
                {
                    output.Append(_boardService.IsAlive(board, row, column) ? "# " : ". ");
                }
                output.AppendLine();
            }

            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
            Console.Write(output.ToString());
        }
    }
}
