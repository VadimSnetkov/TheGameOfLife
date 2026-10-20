using System.Diagnostics;
using FluentValidation;
using GameOfLife.ConsoleApp.Models;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;

namespace GameOfLife.ConsoleApp.UI
{
    public class SessionRunner
    {
        private static readonly TimeSpan GenerationInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan InputPollInterval = TimeSpan.FromMilliseconds(50);
        private readonly ConsoleRenderer _renderer;
        private readonly SessionService _sessionService;
        private readonly GameFileService _fileService;
        private readonly IValidator<ViewSelection> _viewValidator;

        /// Summary:
        /// Receives session evolution, storage, rendering, and view validation.
        public SessionRunner(ConsoleRenderer renderer, SessionService sessionService,
            GameFileService fileService, IValidator<ViewSelection> viewValidator)
        {
            _renderer = renderer;
            _sessionService = sessionService;
            _fileService = fileService;
            _viewValidator = viewValidator;
        }

        /// Summary:
        /// Keeps keyboard input responsive while evolution and saving run in background tasks.
        public async Task RunAsync(GameSession session)
        {
            using var stop = new CancellationTokenSource();
            ConsoleCancelEventHandler cancelHandler = (_, args) =>
            {
                args.Cancel = true;
                stop.Cancel();
            };
            Console.CancelKeyPress += cancelHandler;
            Task<(GameSession Session, SessionStatistics Statistics)>? updateTask = null;
            Task<string>? saveTask = null;
            int[] selectedIds = Enumerable.Range(1, Math.Min(ViewSelection.MaximumVisibleGames, session.Games.Count)).ToArray();
            SessionStatistics statistics = _sessionService.GetStatistics(session);
            string message = "All games run, including games outside the selected views.";
            string? selectionInput = null;
            var clock = Stopwatch.StartNew();
            bool redraw = true;
            (int Width, int Height) lastSize = default;

            try
            {
                Console.Clear();
                while (!stop.IsCancellationRequested)
                {
                    if (updateTask is { IsCompleted: true })
                    {
                        (session, statistics) = await updateTask;
                        updateTask = null;
                        redraw = true;
                    }
                    if (saveTask is { IsCompleted: true })
                    {
                        message = await saveTask;
                        saveTask = null;
                        redraw = true;
                    }

                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Q || (key.Key == ConsoleKey.Escape && selectionInput is null))
                        {
                            break;
                        }
                        if (selectionInput is not null)
                        {
                            if (key.Key == ConsoleKey.Escape)
                            {
                                selectionInput = null;
                            }
                            else if (key.Key == ConsoleKey.Enter)
                            {
                                int[] ids = selectionInput.Split(',').Select(value =>
                                    int.TryParse(value.Trim(), out int id) ? id : 0).ToArray();
                                var result = _viewValidator.Validate(new ViewSelection
                                {
                                    GameIds = ids, GameCount = session.Games.Count
                                });
                                if (result.IsValid)
                                {
                                    selectedIds = ids;
                                    selectionInput = null;
                                    message = $"Showing games: {string.Join(", ", ids)}";
                                }
                                else
                                {
                                    message = result.Errors[0].ErrorMessage;
                                }
                            }
                            else if (key.Key == ConsoleKey.Backspace && selectionInput.Length > 0)
                            {
                                selectionInput = selectionInput[..^1];
                            }
                            else if ((char.IsAsciiDigit(key.KeyChar) || key.KeyChar is ',' or ' ') && selectionInput.Length < 64)
                            {
                                selectionInput += key.KeyChar;
                            }
                        }
                        else if (key.Key == ConsoleKey.V)
                        {
                            selectionInput = string.Empty;
                            message = $"Enter 1 to 8 distinct game numbers from 1 to {session.Games.Count}, separated by commas.";
                        }
                        else if (key.Key == ConsoleKey.S)
                        {
                            if (saveTask is null)
                            {
                                GameSession snapshot = session;
                                saveTask = Task.Run(() => SaveSessionAsync(snapshot, stop.Token), stop.Token);
                                message = $"Saving all {snapshot.Games.Count} games...";
                            }
                            else
                            {
                                message = "A save is already running.";
                            }
                        }
                        redraw = true;
                    }

                    if (updateTask is null && clock.Elapsed >= GenerationInterval)
                    {
                        GameSession snapshot = session;
                        updateTask = Task.Run(() =>
                        {
                            GameSession next = _sessionService.NextIterationStep(snapshot, stop.Token);
                            return (next, _sessionService.GetStatistics(next));
                        }, stop.Token);
                        clock.Restart();
                    }

                    var size = (Console.WindowWidth, Console.WindowHeight);
                    if (size != lastSize)
                    {
                        Console.Clear();
                        lastSize = size;
                        redraw = true;
                    }
                    if (redraw)
                    {
                        _renderer.Render(session, statistics, selectedIds, message, selectionInput);
                        redraw = false;
                    }
                    await Task.Delay(InputPollInterval, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested)
            {
                // Ctrl+C interrupts the input wait and requests cancellation of active work.
            }
            finally
            {
                stop.Cancel();
                try
                {
                    await Task.WhenAll(updateTask ?? Task.CompletedTask, saveTask ?? Task.CompletedTask);
                }
                catch (OperationCanceledException) when (stop.IsCancellationRequested)
                {
                    // Wait for workers to finish so no incomplete save is left running.
                }
                Console.CancelKeyPress -= cancelHandler;
            }

            Console.WriteLine();
            Console.WriteLine("Session stopped. Only completed saves are kept.");
        }

        /// Summary:
        /// Saves the captured session and returns a message without interrupting the simulation.
        private async Task<string> SaveSessionAsync(GameSession snapshot, CancellationToken cancellationToken)
        {
            try
            {
                await _fileService.SaveAllAsync(snapshot, cancellationToken);
                return $"Saved all {snapshot.Games.Count} games to {_fileService.SessionSavePath}";
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                return $"Could not save: {exception.Message}";
            }
        }
    }
}
