<<<<<<< HEAD
using FluentValidation;
using GameOfLife.ConsoleApp.Models;
using GameOfLife.ConsoleApp.Service;

namespace GameOfLife.ConsoleApp.UI
{
    public class ConsoleApplication
    {
        private static readonly BoardRequest SmallBoard = new() { Rows = 10, Columns = 20 };
        private static readonly BoardRequest MediumBoard = new() { Rows = 15, Columns = 30 };
        private static readonly BoardRequest LargeBoard = new() { Rows = 20, Columns = 40 };

        private readonly ConsoleRenderer _renderer;
        private readonly BoardFactory _boardFactory;
        private readonly GameService _gameService;
        private readonly IValidator<MenuRequest> _menuValidator;

        /// Summary:
        /// Receives the renderer, board factory, menu validator, and generation service.
        public ConsoleApplication(ConsoleRenderer renderer, BoardFactory boardFactory,
            IValidator<MenuRequest> menuValidator, GameService gameService)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(boardFactory);
            ArgumentNullException.ThrowIfNull(menuValidator);
            ArgumentNullException.ThrowIfNull(gameService);

            _renderer = renderer;
            _boardFactory = boardFactory;
            _menuValidator = menuValidator;
            _gameService = gameService;
        }

        /// Summary:
        /// Displays the selected board and calculates its next generation on each one-second tick.
        public async Task RunAsync()
        {
            Board? board = SelectBoard();
            if (board is null)
            {
                return;
            }

            _renderer.Render(board);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync())
            {
                board = _gameService.NextIterationStep(board);
                _renderer.Render(board);
            }
        }

        /// Summary:
        /// Validates menu input and creates the selected board, or returns null when exiting.
        private Board? SelectBoard()
        {
            while (true)
            {
                Console.WriteLine("TheGameOfLife");
                Console.WriteLine("Choose a field size (rows x columns):");
                Console.WriteLine($"{(int)MenuChoice.Small}. Small  - {SmallBoard.Rows} x {SmallBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Medium}. Medium - {MediumBoard.Rows} x {MediumBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Large}. Large  - {LargeBoard.Rows} x {LargeBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Exit}. Exit");
                Console.Write("Your choice: ");

                string? input = Console.ReadLine();
                if (input is null)
                {
                    return null;
                }

                var request = new MenuRequest
                {
                    Choice = int.TryParse(input, out int choice)
                        ? (MenuChoice)choice
                        : (MenuChoice)(-1)
                };

                var result = _menuValidator.Validate(request);
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    Console.WriteLine();
                    continue;
                }

                if (request.Choice == MenuChoice.Exit)
                {
                    return null;
                }

                BoardRequest boardRequest = request.Choice switch
                {
                    MenuChoice.Small => SmallBoard,
                    MenuChoice.Medium => MediumBoard,
                    MenuChoice.Large => LargeBoard,
                    _ => throw new InvalidOperationException("The validated menu choice has no board size.")
                };

                return _boardFactory.CreateRandom(boardRequest);
            }
        }
    }
}
=======
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
>>>>>>> origin/main
