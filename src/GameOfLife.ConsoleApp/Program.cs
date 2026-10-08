using GameOfLife.ConsoleApp.UI;
using GameOfLife.ConsoleApp.Validator;
using GameOfLife.Core.Service;
using GameOfLife.Core.Validator;

namespace GameOfLife.ConsoleApp
{
    internal class Program
    {
        /// Summary:
        /// Creates the services and validators, then starts the console application.
        static async Task Main()
        {
            var boardService = new BoardService();
            var boardValidator = new BoardRequestValidator();
            var boardFactory = new BoardFactory(boardValidator, new Random(), boardService);
            var gameService = new GameService(boardService);
            var fileService = new GameFileService(boardService, new GameSaveDataValidator(boardValidator));
            var renderer = new ConsoleRenderer(boardService);

            string savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TheGameOfLife", "game.json");

            var application = new ConsoleApplication(
                renderer, boardFactory, new MenuRequestValidator(), gameService, fileService, savePath);

            await application.RunAsync();
        }
    }
}
