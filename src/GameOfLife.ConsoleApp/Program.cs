using GameOfLife.ConsoleApp.Models;
using GameOfLife.ConsoleApp.UI;

namespace GameOfLife.ConsoleApp
{
    internal class Program
    {
        static async Task Main()
        {
            var renderer = new ConsoleRenderer();
            var boardFactory = new BoardFactory();

            var application = new ConsoleApplication(renderer, boardFactory);
            await application.RunAsync();
        }
    }
}
