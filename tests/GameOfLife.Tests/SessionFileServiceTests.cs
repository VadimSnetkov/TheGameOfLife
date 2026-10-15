using GameOfLife.Core.Models;
using GameOfLife.Core.Service;
using GameOfLife.Core.Validator;
using Microsoft.Extensions.Configuration;

namespace GameOfLife.Tests
{
    public class SessionFileServiceTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "GameOfLifeTests", Guid.NewGuid().ToString("N"));
        private readonly BoardService _boards = new();
        private readonly GameFileService _files;

        /// Summary:
        /// Gives each test separate single-game and multi-game save files.
        public SessionFileServiceTests()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["GameSave:Folder"] = _directory,
                    ["GameSave:FileName"] = "game.json",
                    ["GameSave:SessionFileName"] = "games.json"
                }).Build();
            var validator = new GameSaveDataValidator(new BoardRequestValidator());
            _files = new GameFileService(_boards, validator, new SessionSaveDataValidator(validator), configuration);
            Directory.CreateDirectory(_directory);
        }

        /// Summary:
        /// Restores 1000 different generations and preserves every game's number and cell values.
        [Fact]
        public async Task Test_SaveAll_RoundTrips1000GamesAndKeepsSingleSave()
        {
            await _files.SaveAsync(new GameState(new Board(1, 1), 7));
            string singleSave = await File.ReadAllTextAsync(_files.SavePath);
            var games = Enumerable.Range(0, 1000).Select(index =>
            {
                var board = new Board(3, 5);
                _boards.SetCell(board, index % 3, index % 5, true);
                return new GameState(board, index);
            }).ToArray();

            await _files.SaveAllAsync(new GameSession(games));
            GameSession loaded = await _files.LoadAllAsync();

            Assert.Equal(1000, loaded.Games.Count);
            Assert.All(Enumerable.Range(0, 1000), index =>
            {
                Assert.Equal((long)index, loaded.Games[index].Iteration);
                Assert.Equal(3, loaded.Games[index].Board.Rows);
                Assert.Equal(5, loaded.Games[index].Board.Columns);
                Assert.Equal(1, _boards.CountLivingCells(loaded.Games[index].Board));
                Assert.True(_boards.IsAlive(loaded.Games[index].Board, index % 3, index % 5));
            });
            Assert.Equal(singleSave, await File.ReadAllTextAsync(_files.SavePath));
            Assert.Equal(7L, (await _files.LoadAsync()).Iteration);
        }

        /// Summary:
        /// Uses a session limit that supports 1000 boards at the largest allowed dimensions.
        [Fact]
        public async Task Test_SaveAll_Supports1000MaximumSizeBoards()
        {
            var session = new GameSession(Enumerable.Range(0, 1000)
                .Select(index => new GameState(new Board(100, 200), index)));
            await _files.SaveAllAsync(session);
            Assert.True(new FileInfo(_files.SessionSavePath).Length > 1_000_000);
            GameSession loaded = await _files.LoadAllAsync();
            Assert.Equal(1000, loaded.Games.Count);
            Assert.All(loaded.Games, game =>
            {
                Assert.Equal(100, game.Board.Rows);
                Assert.Equal(200, game.Board.Columns);
                Assert.Equal(0, _boards.CountLivingCells(game.Board));
            });
        }

        /// Summary:
        /// Rejects invalid versions, counts, and individual game data before restoring a session.
        [Theory]
        [InlineData("not JSON")]
        [InlineData("null")]
        [InlineData("{}")]
        [InlineData("{\"Version\":2,\"Games\":[]}")]
        [InlineData("{\"Version\":1,\"Games\":[]}")]
        [InlineData("{\"Version\":1,\"Games\":null}")]
        [InlineData("{\"Version\":1,\"Games\":[null]}")]
        [InlineData("{\"Version\":1,\"Games\":[{\"Rows\":1,\"Columns\":1,\"Iteration\":0,\"Cells\":[]}]}")]
        public async Task Test_LoadAll_RejectsInvalidData(string json)
        {
            await File.WriteAllTextAsync(_files.SessionSavePath, json);
            await Assert.ThrowsAsync<InvalidDataException>(() => _files.LoadAllAsync());
        }

        /// Summary:
        /// Rejects a session containing more games than the application supports.
        [Fact]
        public async Task Test_LoadAll_RejectsTooManyGames()
        {
            string game = "{\"Rows\":1,\"Columns\":1,\"Iteration\":0,\"Cells\":[false]}";
            await File.WriteAllTextAsync(_files.SessionSavePath,
                "{\"Version\":1,\"Games\":[" + string.Join(",", Enumerable.Repeat(game, 1001)) + "]}");
            await Assert.ThrowsAsync<InvalidDataException>(() => _files.LoadAllAsync());
        }

        /// Summary:
        /// Replaces the entire session so games from an older, larger save cannot remain.
        [Fact]
        public async Task Test_SaveAll_ReplacesWholeSession()
        {
            await _files.SaveAllAsync(new GameSession(new[]
            {
                new GameState(new Board(2, 2)), new GameState(new Board(3, 3))
            }));
            await _files.SaveAllAsync(new GameSession(new[] { new GameState(new Board(1, 1), 42) }));
            GameSession loaded = await _files.LoadAllAsync();
            Assert.Single(loaded.Games);
            Assert.Equal(42L, loaded.Games[0].Iteration);
            Assert.Empty(Directory.GetFiles(_directory, "*.tmp"));
        }

        /// Summary:
        /// Preserves the previous session when a save is cancelled or contains an invalid board.
        [Fact]
        public async Task Test_FailedSaveAll_PreservesPreviousSnapshot()
        {
            var session = new GameSession(new[] { new GameState(new Board(1, 1), 9) });
            await _files.SaveAllAsync(session);
            string original = await File.ReadAllTextAsync(_files.SessionSavePath);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _files.SaveAllAsync(session, cancellation.Token));
            await Assert.ThrowsAsync<InvalidDataException>(() => _files.SaveAllAsync(
                new GameSession(new[] { new GameState(new Board(101, 1)) })));
            Assert.Equal(original, await File.ReadAllTextAsync(_files.SessionSavePath));
            Assert.Empty(Directory.GetFiles(_directory, "*.tmp"));
        }

        /// Summary:
        /// Rejects an oversized session before attempting JSON deserialization.
        [Fact]
        public async Task Test_LoadAll_RejectsOversizedFile()
        {
            using (var stream = File.Create(_files.SessionSavePath))
            {
                stream.SetLength(128_000_001);
            }
            await Assert.ThrowsAsync<InvalidDataException>(() => _files.LoadAllAsync());
        }

        /// Summary:
        /// Deletes the isolated test files after the test completes.
        public void Dispose()
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
