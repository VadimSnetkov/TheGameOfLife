using FluentValidation;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;
using GameOfLife.Core.Validator;

namespace GameOfLife.Tests
{
    public class SessionServiceTests
    {
        private readonly BoardService _boards = new();
        private readonly SessionService _sessions;

        /// Summary:
        /// Creates the real session services with deterministic random board creation.
        public SessionServiceTests()
        {
            var validator = new BoardRequestValidator();
            _sessions = new SessionService(new BoardFactory(validator, new Random(17), _boards),
                new GameService(_boards), _boards, new SessionRequestValidator(validator));
        }

        /// Summary:
        /// Verifies that 1000 parallel updates match sequential evolution without modifying the old snapshot.
        [Fact]
        public void Test_ParallelUpdate_MatchesSequentialFor1000Games()
        {
            GameSession original = _sessions.Create(new SessionRequest
            {
                Board = new BoardRequest { Rows = 10, Columns = 20 }, GameCount = 1000
            });
            var gameService = new GameService(_boards);
            GameState[] expected = original.Games.Select(game => gameService.NextIterationStep(game)).ToArray();
            GameSession actual = _sessions.NextIterationStep(original);

            Assert.Equal(1000, actual.Games.Count);
            Assert.All(original.Games, game => Assert.Equal(0L, game.Iteration));
            Assert.All(Enumerable.Range(0, actual.Games.Count), index =>
            {
                Assert.Equal(1L, actual.Games[index].Iteration);
                Assert.NotSame(original.Games[index].Board, actual.Games[index].Board);
                Assert.All(Enumerable.Range(0, 10), row =>
                    Assert.All(Enumerable.Range(0, 20), column =>
                        Assert.Equal(_boards.IsAlive(expected[index].Board, row, column),
                            _boards.IsAlive(actual.Games[index].Board, row, column))));
            });
        }

        /// Summary:
        /// Counts living games by population, including stable patterns and games outside the first eight.
        [Fact]
        public void Test_Statistics_IncludeUnselectedAndStableGames()
        {
            var games = Enumerable.Range(0, 10).Select(_ => new GameState(new Board(4, 4))).ToArray();
            _boards.SetCell(games[0].Board, 1, 1, true);
            _boards.SetCell(games[9].Board, 1, 1, true);
            _boards.SetCell(games[9].Board, 1, 2, true);
            _boards.SetCell(games[9].Board, 2, 1, true);
            _boards.SetCell(games[9].Board, 2, 2, true);
            var session = new GameSession(games);

            Assert.Equal(new SessionStatistics(10, 2, 5), _sessions.GetStatistics(session));
            GameSession next = _sessions.NextIterationStep(session);
            Assert.Equal(new SessionStatistics(10, 1, 4), _sessions.GetStatistics(next));
            Assert.Equal(1L, next.Games[9].Iteration);
            Assert.Equal(5, session.Games.Sum(game => _boards.CountLivingCells(game.Board)));
        }

        /// Summary:
        /// Rejects unsupported game counts before allocating boards.
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1001)]
        public void Test_Create_RejectsInvalidCount(int count)
        {
            Assert.Throws<ValidationException>(() => _sessions.Create(new SessionRequest
            {
                Board = new BoardRequest { Rows = 3, Columns = 3 }, GameCount = count
            }));
        }

        /// Summary:
        /// Verifies that separately created games do not share a mutable board.
        [Fact]
        public void Test_Create_UsesIndependentBoards()
        {
            GameSession session = _sessions.Create(new SessionRequest
            {
                Board = new BoardRequest { Rows = 1, Columns = 1 }, GameCount = 2
            });
            bool second = _boards.IsAlive(session.Games[1].Board, 0, 0);
            _boards.SetCell(session.Games[0].Board, 0, 0, !second);
            Assert.Equal(second, _boards.IsAlive(session.Games[1].Board, 0, 0));
        }

        /// Summary:
        /// Cancels an update without publishing or modifying a partial generation.
        [Fact]
        public void Test_CancelledUpdate_PreservesSnapshot()
        {
            var session = new GameSession(new[] { new GameState(new Board(3, 3), 12) });
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.ThrowsAny<OperationCanceledException>(() => _sessions.NextIterationStep(session, cancellation.Token));
            Assert.Equal(12L, session.Games[0].Iteration);
            Assert.Equal(0, _boards.CountLivingCells(session.Games[0].Board));
        }
    }
}
