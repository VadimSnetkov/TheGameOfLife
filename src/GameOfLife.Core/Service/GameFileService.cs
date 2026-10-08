using System.Text.Json;
using FluentValidation;
using GameOfLife.Core.Models;

namespace GameOfLife.Core.Service
{
    public class GameFileService
    {
        private const long MaximumFileSizeBytes = 1_000_000;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private readonly BoardService _boardService;
        private readonly IValidator<GameSaveData> _validator;

        /// Summary:
        /// Receives cell access and validation used when saving and restoring game states.
        public GameFileService(BoardService boardService, IValidator<GameSaveData> validator)
        {
            ArgumentNullException.ThrowIfNull(boardService);
            ArgumentNullException.ThrowIfNull(validator);
            _boardService = boardService;
            _validator = validator;
        }

        /// Summary:
        /// Writes a complete snapshot to a temporary file before replacing the previous save.
        public async Task SaveAsync(GameState state, string path, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(state);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            cancellationToken.ThrowIfCancellationRequested();

            GameSaveData data = CreateSaveData(state);
            ValidateSaveData(data);

            string fullPath = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            string temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew,
                    FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await JsonSerializer.SerializeAsync(stream, data, JsonOptions, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                File.Move(temporaryPath, fullPath, overwrite: true);
            }
            finally
            {
                DeleteTemporaryFile(temporaryPath);
            }
        }

        /// Summary:
        /// Reads and validates a saved snapshot, then restores its board and iteration number.
        public async Task<GameState> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            cancellationToken.ThrowIfCancellationRequested();

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.Read, 4096, useAsync: true);
            if (stream.Length > MaximumFileSizeBytes)
            {
                throw new InvalidDataException("The save file is too large.");
            }

            GameSaveData? data;
            try
            {
                data = await JsonSerializer.DeserializeAsync<GameSaveData>(stream, JsonOptions, cancellationToken);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The save file is not valid game JSON.", exception);
            }

            if (data is null)
            {
                throw new InvalidDataException("The save file contains no game data.");
            }
            ValidateSaveData(data);

            var board = new Board(data.Rows, data.Columns);
            for (int row = 0; row < board.Rows; row++)
            {
                for (int column = 0; column < board.Columns; column++)
                {
                    _boardService.SetCell(board, row, column, data.Cells[row * board.Columns + column]);
                }
            }

            return new GameState(board, data.Iteration);
        }

        /// Summary:
        /// Converts the two-dimensional board into a row-by-row array supported by JSON.
        private GameSaveData CreateSaveData(GameState state)
        {
            var cells = new bool[checked(state.Board.Rows * state.Board.Columns)];
            for (int row = 0; row < state.Board.Rows; row++)
            {
                for (int column = 0; column < state.Board.Columns; column++)
                {
                    cells[row * state.Board.Columns + column] = _boardService.IsAlive(state.Board, row, column);
                }
            }

            return new GameSaveData
            {
                Rows = state.Board.Rows,
                Columns = state.Board.Columns,
                Iteration = state.Iteration,
                Cells = cells
            };
        }

        /// Summary:
        /// Reports invalid save data as a readable file error before using it.
        private void ValidateSaveData(GameSaveData data)
        {
            var result = _validator.Validate(data);
            if (!result.IsValid)
            {
                throw new InvalidDataException(string.Join(" ", result.Errors.Select(error => error.ErrorMessage)));
            }
        }

        /// Summary:
        /// Removes an unfinished temporary save without hiding the original save error.
        private static void DeleteTemporaryFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // The incomplete temporary file is never used as a saved game.
            }
            catch (UnauthorizedAccessException)
            {
                // The completed save remains separate from this temporary file.
            }
        }
    }
}
