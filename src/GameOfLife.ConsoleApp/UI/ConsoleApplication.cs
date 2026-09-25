using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.UI
{
    public class ConsoleApplication
    {
        private readonly ConsoleRenderer _renderer;
        private readonly BoardFactory _boardFactory;
        private const string ExitChoice = "0";
        private const string SmallBoardChoice = "1";
        private const string MediumBoardChoice = "2";
        private const string LargeBoardChoice = "3";

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

                game.CalculateNextGeneration();
                _renderer.Render(game.CurrentBoard);
            }
        }
        //Starts the selected game and updates it approximately every second.

        private Board? SelectBoard()
        {
            while (true)
            {
                Console.WriteLine("Conway's Game of Life");
                Console.WriteLine("Choose a field size (rows x columns):");
                Console.WriteLine($"{SmallBoardChoice}. Small  - 10 x 20");
                Console.WriteLine($"{MediumBoardChoice}. Medium - 15 x 30");
                Console.WriteLine($"{LargeBoardChoice}. Large  - 20 x 40");
                Console.WriteLine($"{ExitChoice}. Exit");
                Console.Write("Your choice: ");

                string? choice = Console.ReadLine();

                switch (choice?.Trim())
                {
                    case SmallBoardChoice:
                        return _boardFactory.CreateRandom(10, 20);

                    case MediumBoardChoice:
                        return _boardFactory.CreateRandom(15, 30);

                    case LargeBoardChoice:
                        return _boardFactory.CreateRandom(20, 40);

                    case ExitChoice:
                    case null:
                        return null;

                    default:
                        Console.WriteLine();
                        Console.WriteLine(
                            $"Invalid choice. Enter {ExitChoice}, " +
                            $"{SmallBoardChoice}, {MediumBoardChoice}, " +
                            $"or {LargeBoardChoice}.");
                        Console.WriteLine();
                        break;
                }
            }

            // Summary: Prompts for a field size and returns a random board, or null to exit.
        }
        //Prompts for a field size and returns a random board, or null to exit.
    }
}
