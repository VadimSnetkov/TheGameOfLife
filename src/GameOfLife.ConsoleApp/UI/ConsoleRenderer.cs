using System.Text;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;

namespace GameOfLife.ConsoleApp.UI
{
    public class ConsoleRenderer
    {
        private readonly BoardService _boardService;

        /// Summary:
        /// Receives cell access used to draw selected boards and their populations.
        public ConsoleRenderer(BoardService boardService)
        {
            ArgumentNullException.ThrowIfNull(boardService);
            _boardService = boardService;
        }

        /// Summary:
        /// Repaints the dashboard; large boards show their top-left area without changing the simulation.
        public void Render(GameSession session, SessionStatistics statistics,
            IReadOnlyList<int> selectedIds, string message, string? selectionInput = null)
        {
            int width = Console.IsOutputRedirected ? 100 : Math.Max(20, Console.WindowWidth - 1);
            int height = Console.IsOutputRedirected ? 40 : Math.Max(8, Console.WindowHeight - 1);
            string[] lines = BuildFrame(session, statistics, selectedIds, message, selectionInput, width, height);
            if (!Console.IsOutputRedirected)
            {
                Console.SetCursorPosition(0, 0);
            }
            const int SelectionPromptLineIndex = 4;
            ConsoleColor originalColor = Console.ForegroundColor;

            try
            {
                for (int index = 0; index < lines.Length; index++)
                {
                    if (index > 0)
                    {
                        Console.Write(Environment.NewLine);
                    }

                    Console.ForegroundColor =
                        selectionInput is not null && index == SelectionPromptLineIndex
                            ? ConsoleColor.Cyan
                            : originalColor;

                    Console.Write(lines[index]);
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        /// Summary:
        /// Builds a bounded grid of game panels and whole-session statistics for the available screen.
        public string[] BuildFrame(GameSession session, SessionStatistics statistics,
            IReadOnlyList<int> selectedIds, string message, string? selectionInput, int width, int height)
        {
            var lines = new List<string>
            {
                "TheGameOfLife - parallel games",
                $"Games: {statistics.TotalGames} | Live games: {statistics.LiveGames} | Living cells: {statistics.LivingCells}",
                "V: select views | S: save all | Q / Esc / Ctrl+C: exit",
                message,
                selectionInput is null
                    ? "Panel: status | living cells. View = top-left area shown."
                    : $"View IDs: {selectionInput}_  [Enter: apply / Esc: cancel]"
            };

            int columns = Math.Min(selectedIds.Count, Math.Min(4, Math.Max(1, width / 19)));
            int panelRows = (selectedIds.Count + columns - 1) / columns;
            int panelWidth = (width - (columns - 1)) / columns;
            int panelHeight = (height - lines.Count) / panelRows;
            if (panelWidth < 18 || panelHeight < 4)
            {
                lines.Add("Enlarge terminal to at least 80 x 24 to show eight views.");
            }
            else
            {
                for (int panelRow = 0; panelRow < panelRows; panelRow++)
                {
                    for (int line = 0; line < panelHeight; line++)
                    {
                        var text = new StringBuilder();
                        for (int column = 0; column < columns; column++)
                        {
                            int index = panelRow * columns + column;
                            string content = index < selectedIds.Count
                                ? BuildPanelLine(session, selectedIds[index], line, panelWidth, panelHeight)
                                : string.Empty;
                            if (column > 0)
                            {
                                text.Append(' ');
                            }
                            text.Append(Fit(content, panelWidth));
                        }
                        lines.Add(text.ToString());
                    }
                }
            }

            while (lines.Count < height)
            {
                lines.Add(string.Empty);
            }
            return lines.Take(height).Select(line => Fit(line, width)).ToArray();
        }

        /// Summary:
        /// Formats one panel line with the game number, iteration, population, or visible cells.
        private string BuildPanelLine(GameSession session, int id, int line, int width, int height)
        {
            GameState game = session.Games[id - 1];
            int visibleRows = Math.Min(game.Board.Rows, height - 3);
            int visibleColumns = Math.Min(game.Board.Columns, width);
            if (line == 0)
            {
                return $"Game {id} | Gen {game.Iteration}";
            }
            if (line == 1)
            {
                int livingCells = _boardService.CountLivingCells(game.Board);

                string status = livingCells == 0
                    ? "Dead"
                    : game.IsStagnant ? "Stagnant" : "Living";

                return $"{status} | {livingCells}";
            }
            if (line == height - 1)
            {
                return $"View {visibleRows}x{visibleColumns}";
            }
            int row = line - 2;
            if (row >= visibleRows)
            {
                return string.Empty;
            }

            var cells = new StringBuilder(visibleColumns);
            for (int column = 0; column < visibleColumns; column++)
            {
                cells.Append(_boardService.IsAlive(game.Board, row, column) ? '#' : '.');
            }
            return cells.ToString();
        }

        /// Summary:
        /// Fits each line to its allotted width so old text is erased and panels cannot wrap.
        private static string Fit(string text, int width)
        {
            return text.Length > width ? text[..width] : text.PadRight(width);
        }
    }
}
