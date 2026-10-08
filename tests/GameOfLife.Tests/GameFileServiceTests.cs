using GameOfLife.Core.Models;
using GameOfLife.Core.Service;
using GameOfLife.Core.Validator;

namespace GameOfLife.Tests
{
    public class GameFileServiceTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "GameOfLifeTests", Guid.NewGuid().ToString("N"));
        private readonly BoardService _boardService = new();
        private readonly GameFileService _fileService;
        private readonly string _path;

        /// Summary:
        /// Creates an isolated save location and the real file service for each test.
        public GameFileServiceTests()
        {
            Directory.CreateDirectory(_directory);
            _path = Path.Combine(_directory, "nested", "game.json");
            _fileService = new GameFileService(_boardService,
                new GameSaveDataValidator(new BoardRequestValidator()));
        }

        /// Summary:
        /// Restores every cell and the iteration number, then continues the same simulation.
        [Fact]
        public async Task Test_SaveAndLoad_PreservesStateAndAllowsContinuation()
        {
            var board = new Board(3, 5);
            _boardService.SetCell(board, 1, 1, true);
            _boardService.SetCell(board, 1, 2, true);
            _boardService.SetCell(board, 1, 3, true);
            var state = new GameState(board, 27);

            await _fileService.SaveAsync(state, _path);
            GameState loaded = await _fileService.LoadAsync(_path);

            Assert.Equal(3, loaded.Board.Rows);
            Assert.Equal(5, loaded.Board.Columns);
            Assert.Equal(27L, loaded.Iteration);
            AssertBoardsEqual(state.Board, loaded.Board);
            var game = new GameService(_boardService);
            GameState next = game.NextIterationStep(loaded);
            Assert.Equal(28L, next.Iteration);
            AssertBoardsEqual(game.NextIterationStep(state).Board, next.Board);
        }

        /// Summary:
        /// Checks that the largest allowed board can be saved and loaded within the file-size limit.
        [Fact]
        public async Task Test_SaveAndLoad_SupportsMaximumBoardSize()
        {
            var state = new GameState(new Board(100, 200));

            await _fileService.SaveAsync(state, _path);
            GameState loaded = await _fileService.LoadAsync(_path);

            Assert.Equal(100, loaded.Board.Rows);
            Assert.Equal(200, loaded.Board.Columns);
            Assert.Equal(0, _boardService.CountLivingCells(loaded.Board));
        }

        /// Summary:
        /// Replaces an older save with the latest complete game state.
        [Fact]
        public async Task Test_Save_OverwritesPreviousSnapshot()
        {
            await _fileService.SaveAsync(new GameState(new Board(2, 3), 1), _path);
            var board = new Board(1, 1);
            _boardService.SetCell(board, 0, 0, true);

            await _fileService.SaveAsync(new GameState(board, 8), _path);
            GameState loaded = await _fileService.LoadAsync(_path);

            Assert.Equal(8L, loaded.Iteration);
            Assert.Equal(1, loaded.Board.Rows);
            Assert.Equal(1, loaded.Board.Columns);
            Assert.True(_boardService.IsAlive(loaded.Board, 0, 0));
            Assert.Empty(Directory.GetFiles(_directory, "*.tmp", SearchOption.AllDirectories));
        }

        /// Summary:
        /// Rejects malformed JSON, missing fields, invalid dimensions, and inconsistent cell data.
        [Theory]
        [InlineData("not JSON")]
        [InlineData("null")]
        [InlineData("{}")]
        [InlineData("{\"Rows\":1,\"Columns\":1,\"Iteration\":0}")]
        [InlineData("{\"Rows\":0,\"Columns\":1,\"Iteration\":0,\"Cells\":[]}")]
        [InlineData("{\"Rows\":101,\"Columns\":1,\"Iteration\":0,\"Cells\":[]}")]
        [InlineData("{\"Rows\":1,\"Columns\":201,\"Iteration\":0,\"Cells\":[]}")]
        [InlineData("{\"Rows\":1,\"Columns\":1,\"Iteration\":-1,\"Cells\":[false]}")]
        [InlineData("{\"Rows\":1,\"Columns\":1,\"Iteration\":9223372036854775807,\"Cells\":[false]}")]
        [InlineData("{\"Rows\":1,\"Columns\":2,\"Iteration\":0,\"Cells\":[true]}")]
        [InlineData("{\"Rows\":1,\"Columns\":1,\"Iteration\":0,\"Cells\":null}")]
        public async Task Test_Load_RejectsInvalidSave(string json)
        {
            string path = Path.Combine(_directory, "invalid.json");
            await File.WriteAllTextAsync(path, json);

            await Assert.ThrowsAsync<InvalidDataException>(() => _fileService.LoadAsync(path));
        }

        /// Summary:
        /// Reports a missing save file without creating an empty replacement.
        [Fact]
        public async Task Test_Load_ReportsMissingFile()
        {
            string path = Path.Combine(_directory, "missing.json");

            await Assert.ThrowsAsync<FileNotFoundException>(() => _fileService.LoadAsync(path));
            Assert.False(File.Exists(path));
        }

        /// Summary:
        /// Rejects an oversized file before deserializing it.
        [Fact]
        public async Task Test_Load_RejectsOversizedFile()
        {
            string path = Path.Combine(_directory, "oversized.json");
            await File.WriteAllTextAsync(path, new string(' ', 1_000_001));

            await Assert.ThrowsAsync<InvalidDataException>(() => _fileService.LoadAsync(path));
        }

        /// Summary:
        /// Leaves a previous save unchanged when cancellation is already requested.
        [Fact]
        public async Task Test_CancelledSave_PreservesPreviousFile()
        {
            var state = new GameState(new Board(1, 1), 4);
            await _fileService.SaveAsync(state, _path);
            string original = await File.ReadAllTextAsync(_path);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                _fileService.SaveAsync(new GameState(new Board(2, 2)), _path, cancellation.Token));

            Assert.Equal(original, await File.ReadAllTextAsync(_path));
            Assert.Empty(Directory.GetFiles(_directory, "*.tmp", SearchOption.AllDirectories));
        }

        /// Summary:
        /// Keeps the old file when a new state fails validation.
        [Fact]
        public async Task Test_InvalidSave_PreservesPreviousFile()
        {
            await _fileService.SaveAsync(new GameState(new Board(1, 1), 4), _path);
            string original = await File.ReadAllTextAsync(_path);

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                _fileService.SaveAsync(new GameState(new Board(101, 1)), _path));

            Assert.Equal(original, await File.ReadAllTextAsync(_path));
        }

        /// Summary:
        /// Removes the temporary snapshot if the destination cannot be replaced.
        [Fact]
        public async Task Test_FailedReplacement_RemovesTemporaryFile()
        {
            Directory.CreateDirectory(_path);

            Exception? exception = await Record.ExceptionAsync(() =>
                _fileService.SaveAsync(new GameState(new Board(1, 1)), _path));

            Assert.True(exception is IOException or UnauthorizedAccessException);
            Assert.True(Directory.Exists(_path));
            Assert.Empty(Directory.GetFiles(_directory, "*.tmp", SearchOption.AllDirectories));
        }

        /// Summary:
        /// Compares all cell values on two boards with matching dimensions.
        private void AssertBoardsEqual(Board expected, Board actual)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Columns, actual.Columns);
            Assert.All(Enumerable.Range(0, expected.Rows), row =>
                Assert.All(Enumerable.Range(0, expected.Columns), column =>
                    Assert.Equal(_boardService.IsAlive(expected, row, column),
                        _boardService.IsAlive(actual, row, column))));
        }

        /// Summary:
        /// Deletes the temporary test directory after each test completes.
        public void Dispose()
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
