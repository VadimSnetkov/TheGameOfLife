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
        private readonly IValidator<MenuRequest> _menuValidator;
        private readonly SessionService _sessionService;
        private readonly GameFileService _fileService;
        private readonly SessionRunner _runner;

        /// Summary:
        /// Receives startup validation, session creation, file loading, and the interactive runner.
        public ConsoleApplication(IValidator<MenuRequest> menuValidator, SessionService sessionService,
            GameFileService fileService, SessionRunner runner)
        {
            _menuValidator = menuValidator;
            _sessionService = sessionService;
            _fileService = fileService;
            _runner = runner;
        }

        /// Summary:
        /// Selects a new or saved session and starts the interactive dashboard.
        public async Task RunAsync()
        {
            if (Console.IsInputRedirected || Console.IsOutputRedirected)
            {
                Console.WriteLine("Run the game in an interactive terminal so keyboard controls are available.");
                return;
            }

            GameSession? session = await SelectSessionAsync();
            if (session is not null)
            {
                await _runner.RunAsync(session);
            }
        }

        /// Summary:
        /// Offers new sessions, legacy single-game saves, and saved multi-game sessions.
        private async Task<GameSession?> SelectSessionAsync()
        {
            while (true)
            {
                Console.WriteLine("TheGameOfLife");
                Console.WriteLine($"{(int)MenuChoice.Small}. Small  - {SmallBoard.Rows} x {SmallBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Medium}. Medium - {MediumBoard.Rows} x {MediumBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Large}. Large  - {LargeBoard.Rows} x {LargeBoard.Columns}");
                Console.WriteLine($"{(int)MenuChoice.Custom}. Custom size");
                Console.WriteLine($"{(int)MenuChoice.Load}. Load Iteration 2 single-game save");
                Console.WriteLine($"{(int)MenuChoice.LoadSession}. Load saved session (all games)");
                Console.WriteLine($"{(int)MenuChoice.Exit}. Exit");
                Console.WriteLine($"Session file: {_fileService.SessionSavePath}");
                Console.Write("Your choice: ");
                string? input = Console.ReadLine();
                if (input is null)
                {
                    return null;
                }
                var request = new MenuRequest
                {
                    Choice = int.TryParse(input, out int choice) ? (MenuChoice)choice : (MenuChoice)(-1)
                };
                var result = _menuValidator.Validate(request);
                if (!result.IsValid)
                {
                    Console.WriteLine(result.Errors[0].ErrorMessage);
                    continue;
                }
                if (request.Choice == MenuChoice.Exit)
                {
                    return null;
                }

                try
                {
                    if (request.Choice == MenuChoice.Load)
                    {
                        return new GameSession(new[] { await _fileService.LoadAsync() });
                    }
                    if (request.Choice == MenuChoice.LoadSession)
                    {
                        return await _fileService.LoadAllAsync();
                    }

                    BoardRequest? board = request.Choice == MenuChoice.Custom
                        ? ReadCustomSize()
                        : request.Choice switch
                        {
                            MenuChoice.Small => SmallBoard,
                            MenuChoice.Medium => MediumBoard,
                            MenuChoice.Large => LargeBoard,
                            _ => throw new InvalidOperationException("This choice has no board size.")
                        };
                    if (board is null)
                    {
                        return null;
                    }
                    Console.Write($"Number of games (1-{GameSession.MaximumGames}): ");
                    string? countInput = Console.ReadLine();
                    if (countInput is null)
                    {
                        return null;
                    }
                    var sessionRequest = new SessionRequest
                    {
                        Board = board, GameCount = int.TryParse(countInput, out int count) ? count : 0
                    };
                    return _sessionService.Create(sessionRequest);
                }
                catch (ValidationException exception)
                {
                    Console.WriteLine(string.Join(" ", exception.Errors.Select(error => error.ErrorMessage)));
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
                {
                    Console.WriteLine($"Could not load: {exception.Message}");
                }
            }
        }

        /// Summary:
        /// Reads custom dimensions; request validation handles invalid numbers and unsupported sizes.
        private static BoardRequest? ReadCustomSize()
        {
            Console.Write("Rows: ");
            string? rows = Console.ReadLine();
            if (rows is null)
            {
                return null;
            }
            Console.Write("Columns: ");
            string? columns = Console.ReadLine();
            return columns is null ? null : new BoardRequest
            {
                Rows = int.TryParse(rows, out int rowCount) ? rowCount : 0,
                Columns = int.TryParse(columns, out int columnCount) ? columnCount : 0
            };
        }
    }
}
