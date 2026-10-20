using System.Text.Json;
using FluentValidation;
using GameOfLife.Core.Models;
using Microsoft.Extensions.Configuration;

namespace GameOfLife.Core.Service
{
    public class GameFileService
    {
        private const long MaximumFileSizeBytes = 1_000_000;
        private const long MaximumSessionFileSizeBytes = 128_000_000;
        private static readonly JsonSerializerOptions JsonOptions = new();
        private readonly BoardService _boardService;
        private readonly IValidator<GameSaveData> _validator;
        private readonly IValidator<SessionSaveData> _sessionValidator;
        public string SavePath { get; }
        public string SessionSavePath { get; }

        /// Summary:
        /// Receives validation and builds separate single-game and session paths from configuration.
        public GameFileService(BoardService boardService, IValidator<GameSaveData> validator,
            IValidator<SessionSaveData> sessionValidator, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(boardService);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(sessionValidator);
            ArgumentNullException.ThrowIfNull(configuration);
            _boardService = boardService;
            _validator = validator;
            _sessionValidator = sessionValidator;

            string? folder = configuration["GameSave:Folder"];
            string? fileName = configuration["GameSave:FileName"];
            string? sessionFileName = configuration["GameSave:SessionFileName"];
            ArgumentException.ThrowIfNullOrWhiteSpace(folder);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(sessionFileName);

            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folder);
            SavePath = Path.GetFullPath(Path.Combine(directory, fileName));
            SessionSavePath = Path.GetFullPath(Path.Combine(directory, sessionFileName));
            if (string.Equals(SavePath, SessionSavePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Single-game and session saves must use different file names.");
            }
        }

        /// Summary:
        /// Saves one game using the file format introduced in Iteration 2.
        public async Task SaveAsync(GameState state, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(state);
            cancellationToken.ThrowIfCancellationRequested();
            GameSaveData data = CreateSaveData(state, cancellationToken);
            ValidateData(data, _validator);
            await WriteAsync(SavePath, data, cancellationToken);
        }

        /// Summary:
        /// Restores and validates an Iteration 2 single-game save.
        public async Task<GameState> LoadAsync(CancellationToken cancellationToken = default)
        {
            GameSaveData data = await ReadAsync(
                SavePath, MaximumFileSizeBytes, _validator, cancellationToken);
            return RestoreGame(data, cancellationToken);
        }

        /// Summary:
        /// Saves every game from the same completed session snapshot in one replacement file.
        public async Task SaveAllAsync(GameSession session, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(session);
            cancellationToken.ThrowIfCancellationRequested();
            var games = new GameSaveData[session.Games.Count];
            for (int index = 0; index < games.Length; index++)
            {
                games[index] = CreateSaveData(session.Games[index], cancellationToken);
            }

            var data = new SessionSaveData { Version = 1, Games = games };
            ValidateData(data, _sessionValidator);
            await WriteAsync(SessionSavePath, data, cancellationToken);
        }

        /// Summary:
        /// Restores all boards, iteration counts, and game numbers from a validated session file.
        public async Task<GameSession> LoadAllAsync(CancellationToken cancellationToken = default)
        {
            SessionSaveData data = await ReadAsync(
                SessionSavePath, MaximumSessionFileSizeBytes, _sessionValidator, cancellationToken);
            var games = new GameState[data.Games.Length];
            for (int index = 0; index < games.Length; index++)
            {
                games[index] = RestoreGame(data.Games[index], cancellationToken);
            }

            return new GameSession(games);
        }

        /// Summary:
        /// Reads a bounded JSON file and validates its complete contents before returning them.
        private static async Task<T> ReadAsync<T>(string path, long maximumBytes,
            IValidator<T> validator, CancellationToken cancellationToken) where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await using var stream = new FileStream(path, FileMode.Open,
                    FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                if (stream.Length > maximumBytes)
                {
                    throw new InvalidDataException("The save file is too large.");
                }

                T? data = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
                if (data is null)
                {
                    throw new InvalidDataException("The save file contains no game data.");
                }

                ValidateData(data, validator);
                return data;
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The save file is not valid game JSON.", exception);
            }
        }

        /// Summary:
        /// Writes to a temporary file so a failed or cancelled save preserves the previous snapshot.
        private static async Task WriteAsync<T>(string path, T data, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew,
                    FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await JsonSerializer.SerializeAsync(stream, data, JsonOptions, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                File.Move(temporaryPath, path, overwrite: true);
            }
            finally
            {
                DeleteTemporaryFile(temporaryPath);
            }
        }

        /// Summary:
        /// Flattens a board into row-by-row cell values for JSON storage.
        private GameSaveData CreateSaveData(GameState state, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cells = new bool[checked(state.Board.Rows * state.Board.Columns)];
            for (int row = 0; row < state.Board.Rows; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int column = 0; column < state.Board.Columns; column++)
                {
                    cells[row * state.Board.Columns + column] = _boardService.IsAlive(state.Board, row, column);
                }
            }

            return new GameSaveData
            {
                Rows = state.Board.Rows, Columns = state.Board.Columns,
                Iteration = state.Iteration, Cells = cells
            };
        }

        /// Summary:
        /// Rebuilds a board from already validated cell data.
        private GameState RestoreGame(GameSaveData data, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var board = new Board(data.Rows, data.Columns);
            for (int row = 0; row < board.Rows; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int column = 0; column < board.Columns; column++)
                {
                    _boardService.SetCell(board, row, column, data.Cells[row * board.Columns + column]);
                }
            }

            return new GameState(board, data.Iteration);
        }

        /// Summary:
        /// Converts validation failures into file errors that the console can display.
        private static void ValidateData<T>(T data, IValidator<T> validator)
        {
            var result = validator.Validate(data);
            if (!result.IsValid)
            {
                throw new InvalidDataException(string.Join(" ", result.Errors.Select(error => error.ErrorMessage)));
            }
        }

        /// Summary:
        /// Attempts to remove unfinished output without hiding the original file error.
        private static void DeleteTemporaryFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // An incomplete temporary file is never used as a save.
            }
            catch (UnauthorizedAccessException)
            {
                // The completed save remains separate from this temporary file.
            }
        }
    }
}
