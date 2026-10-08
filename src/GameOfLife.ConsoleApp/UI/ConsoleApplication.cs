using System.Diagnostics;
using FluentValidation;
using GameOfLife.ConsoleApp.Models;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;

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
        private readonly GameFileService _fileService;
        private readonly string _savePath;
        private static readonly TimeSpan GenerationInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan InputPollInterval = TimeSpan.FromMilliseconds(50);

        /// Summary:
        /// Receives the game services, renderer, menu validator, and location of the save file.
        public ConsoleApplication(ConsoleRenderer renderer, BoardFactory boardFactory,
            IValidator<MenuRequest> menuValidator, GameService gameService,
            GameFileService fileService, string savePath)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(boardFactory);
            ArgumentNullException.ThrowIfNull(menuValidator);
            ArgumentNullException.ThrowIfNull(gameService);
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentException.ThrowIfNullOrWhiteSpace(savePath);

            _renderer = renderer;
            _boardFactory = boardFactory;
            _menuValidator = menuValidator;
            _gameService = gameService;
            _fileService = fileService;
            _savePath = savePath;
        }

        /// Summary:
        /// Starts or restores a game and handles saving, stopping, and one-second generation updates.
        public async Task RunAsync()
        {
            if (Console.IsInputRedirected)
            {
                Console.WriteLine("Run the game in an interactive terminal so keyboard controls are available.");
                return;
            }

            GameState? state = await SelectGameAsync();
            if (state is null)
            {
                return;
            }

            using var stop = new CancellationTokenSource();
            ConsoleCancelEventHandler cancelHandler = (_, args) =>
            {
                args.Cancel = true;
                stop.Cancel();
            };
            Console.CancelKeyPress += cancelHandler;

            try
            {
                _renderer.Render(state);
                var clock = Stopwatch.StartNew();
                string? message = null;

                while (!stop.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey key = Console.ReadKey(intercept: true).Key;
                        if (key is ConsoleKey.Q or ConsoleKey.Escape)
                        {
                            break;
                        }

                        if (key == ConsoleKey.S)
                        {
                            message = await SaveGameAsync(state, stop.Token);
                            _renderer.Render(state, message);
                        }
                    }

                    stop.Token.ThrowIfCancellationRequested();
                    if (clock.Elapsed >= GenerationInterval)
                    {
                        state = _gameService.NextIterationStep(state);
                        _renderer.Render(state, message);
                        clock.Restart();
                    }

                    await Task.Delay(InputPollInterval, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested)
            {
                // Ctrl+C cancels both the short input wait and an active file write.
            }
            finally
            {
                Console.CancelKeyPress -= cancelHandler;
            }

            Console.WriteLine("Game stopped.");
        }

        /// Summary:
        /// Saves the displayed state and returns a success or error message without ending the game.
        private async Task<string> SaveGameAsync(GameState state, CancellationToken cancellationToken)
        {
            try
            {
                await _fileService.SaveAsync(state, _savePath, cancellationToken);
                return $"Saved iteration {state.Iteration} to {_savePath}";
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                return $"Could not save: {exception.Message}";
            }
        }

        /// Summary:
        /// Offers new games and loading an existing save, returning null when the user exits.
        private async Task<GameState?> SelectGameAsync()
        {
            while (true)
            {
                Console.WriteLine("TheGameOfLife");
                Console.WriteLine("Choose a field size (rows x columns):");
                Console.WriteLine($"{(int)MenuChoice.Small}. Small  - {SmallBoard.Rows} x {SmallBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Medium}. Medium - {MediumBoard.Rows} x {MediumBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Large}. Large  - {LargeBoard.Rows} x {LargeBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Custom}. Custom size");
                Console.WriteLine($"{(int)MenuChoice.Load}. Load saved game");
                Console.WriteLine($"{(int)MenuChoice.Exit}. Exit");
                Console.WriteLine($"Save file: {_savePath}");
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

                if (request.Choice == MenuChoice.Load)
                {
                    try
                    {
                        return await _fileService.LoadAsync(_savePath);
                    }
                    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
                    {
                        Console.WriteLine($"Could not load: {exception.Message}");
                        continue;
                    }
                }

                if (request.Choice == MenuChoice.Custom)
                {
                    Board? board = SelectCustomBoard();
                    return board is null ? null : new GameState(board);
                }

                BoardRequest boardRequest = request.Choice switch
                {
                    MenuChoice.Small => SmallBoard,
                    MenuChoice.Medium => MediumBoard,
                    MenuChoice.Large => LargeBoard,
                    _ => throw new InvalidOperationException("The validated menu choice has no board size.")
                };

                return new GameState(_boardFactory.CreateRandom(boardRequest));
            }
        }

        /// Summary:
        /// Reads custom dimensions and retries invalid input until a board is created or input ends.
        private Board? SelectCustomBoard()
        {
            while (true)
            {
                Console.Write("Rows: ");
                string? rowsInput = Console.ReadLine();
                if (rowsInput is null)
                {
                    return null;
                }

                Console.Write("Columns: ");
                string? columnsInput = Console.ReadLine();
                if (columnsInput is null)
                {
                    return null;
                }

                if (!int.TryParse(rowsInput, out int rows) ||
                    !int.TryParse(columnsInput, out int columns))
                {
                    Console.WriteLine("Enter whole numbers for rows and columns.");
                    continue;
                }

                var request = new BoardRequest { Rows = rows, Columns = columns };
                try
                {
                    return _boardFactory.CreateRandom(request);
                }
                catch (ValidationException exception)
                {
                    foreach (var error in exception.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }
        }
    }
}
