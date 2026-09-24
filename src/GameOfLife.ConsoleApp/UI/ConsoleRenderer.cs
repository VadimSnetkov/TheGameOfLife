using System.Text;
using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.UI
{
    public class ConsoleRenderer
    {
        public void Render(Board board)
        {
            var output = new StringBuilder();

            output.AppendLine("CThe Game of Life");
            output.AppendLine("# = alive, . = dead | Ctrl+C to exit");
            output.AppendLine();

            for (int row = 0; row < board.Rows; row++)
            {
                for (int column = 0; column < board.Columns; column++)
                {
                    output.Append(board.IsAlive(row, column) ? "# " : ". ");
                }

                output.AppendLine();
            }

            Console.Clear();
            Console.Write(output.ToString());
        }
    }
}
