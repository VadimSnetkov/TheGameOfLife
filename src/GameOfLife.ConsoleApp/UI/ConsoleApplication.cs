using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.UI
{
    public class ConsoleApplication
    {
        private readonly ConsoleRenderer _renderer;
        private readonly BoardFactory _boardFactory;

        public ConsoleApplication(
            ConsoleRenderer renderer,
            BoardFactory boardFactory)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(boardFactory);

            _renderer = renderer;
            _boardFactory = boardFactory;
        }

        public async Task RunAsync()
        {
            //result can be a board or null, so we need to handle the case where the user chooses to exit
            Board? board = SelectBoard();

            if (board is null)
            {
                return;
            }

            var game = new Game(board);

            _renderer.Render(game.CurrentBoard);

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.2));

                game.Advance();
                _renderer.Render(game.CurrentBoard);
            }
        }

        private Board? SelectBoard()
        {
            while (true)
            {
                Console.WriteLine("The Game of Life");
                Console.WriteLine("Choose a field size (rows x columns):");
                Console.WriteLine("1. Small  - 10 x 20");
                Console.WriteLine("2. Medium - 15 x 30");
                Console.WriteLine("3. Large  - 20 x 40");
                Console.WriteLine("0. Exit");
                Console.Write("Choice: ");

                string? choice = Console.ReadLine();

                switch (choice?.Trim())
                {
                    case "1":
                        return _boardFactory.CreateRandom(10, 20);

                    case "2":
                        return _boardFactory.CreateRandom(15, 30);

                    case "3":
                        return _boardFactory.CreateRandom(20, 40);

                    case "0":
                    case null:
                        return null;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Invalid choice.");
                        Console.WriteLine();
                        break;
                }
            }
        }
    }
}
