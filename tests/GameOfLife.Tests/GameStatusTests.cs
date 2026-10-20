using GameOfLife.ConsoleApp.UI;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;

namespace GameOfLife.Tests
{
    public class GameStatusTests
    {
        private readonly BoardService _boards = new();

        /// Summary:
        /// Recognizes a living block as stagnant after one unchanged generation.
        [Fact]
        public void Test_Block_IsStagnant()
        {
            var board = new Board(4, 4);
            _boards.SetCell(board, 1, 1, true);
            _boards.SetCell(board, 1, 2, true);
            _boards.SetCell(board, 2, 1, true);
            _boards.SetCell(board, 2, 2, true);
            var gameService = new GameService(_boards);

            GameState next = gameService.NextIterationStep(new GameState(board));

            Assert.True(next.IsStagnant);
            Assert.Equal(4, _boards.CountLivingCells(next.Board));
            Assert.Equal(1L, next.Iteration);
            Assert.True(gameService.NextIterationStep(next).IsStagnant);
            Assert.Contains("Stagnant | 4", Render(next));
        }

        /// Summary:
        /// Keeps an oscillator running even though its population stays constant.
        [Fact]
        public void Test_Blinker_IsNotStagnant()
        {
            var board = new Board(5, 5);
            _boards.SetCell(board, 2, 1, true);
            _boards.SetCell(board, 2, 2, true);
            _boards.SetCell(board, 2, 3, true);
            var gameService = new GameService(_boards);
            var start = new GameState(board);

            GameState first = gameService.NextIterationStep(start);
            GameState second = gameService.NextIterationStep(first);

            Assert.False(first.IsStagnant);
            Assert.False(second.IsStagnant);
            Assert.Equal(3, _boards.CountLivingCells(first.Board));
            Assert.True(_boards.AreEqual(start.Board, second.Board));
            Assert.Contains("Living | 3", Render(first));
        }

        /// Summary:
        /// Shows an extinct board as dead rather than stagnant, including later unchanged generations.
        [Fact]
        public void Test_Extinction_ShowsDead()
        {
            var board = new Board(3, 3);
            _boards.SetCell(board, 1, 1, true);
            var gameService = new GameService(_boards);

            GameState next = gameService.NextIterationStep(new GameState(board));
            GameState later = gameService.NextIterationStep(next);

            Assert.False(next.IsStagnant);
            Assert.False(later.IsStagnant);
            Assert.Contains("Dead | 0", Render(next));
            Assert.Contains("Dead | 0", Render(later));
            Assert.Contains("Dead | 0", Render(new GameState(new Board(3, 3))));
        }

        /// Summary:
        /// Rejects equal-sized populations with different dimensions as identical boards.
        [Fact]
        public void Test_AreEqual_RequiresMatchingDimensions()
        {
            Assert.False(_boards.AreEqual(new Board(2, 3), new Board(3, 2)));
        }

        /// Summary:
        /// Renders eight copies of a state to verify its status fits the smallest dashboard panels.
        private string Render(GameState state)
        {
            int livingCells = _boards.CountLivingCells(state.Board);
            var renderer = new ConsoleRenderer(_boards);
            var session = new GameSession(Enumerable.Repeat(state, 8));
            string[] frame = renderer.BuildFrame(session,
                new SessionStatistics(8, livingCells == 0 ? 0 : 8, livingCells * 8),
                Enumerable.Range(1, 8).ToArray(), "Ready", null, 79, 23);
            Assert.All(frame, line => Assert.Equal(79, line.Length));
            return string.Join("\n", frame);
        }
    }
}
