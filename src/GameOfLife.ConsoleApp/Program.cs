using GameOfLife.ConsoleApp.Service;
using GameOfLife.ConsoleApp.UI;
using GameOfLife.ConsoleApp.Validator;

namespace GameOfLife.ConsoleApp
{
    internal class Program
    {
        /// Summary:
        /// Creates the services and validators, then starts the application.
        static async Task Main()
        {
            var boardService = new BoardService();

            var boardFactory = new BoardFactory(
                new BoardRequestValidator(),
                new Random(),
                boardService);

            var gameService = new GameService(boardService);
            var renderer = new ConsoleRenderer(boardService);

            var application = new ConsoleApplication(
                renderer,
                boardFactory,
                new MenuRequestValidator(),
                gameService);

            await application.RunAsync();
        }
        //Creates the application dependencies and starts the console application.
    }
}
