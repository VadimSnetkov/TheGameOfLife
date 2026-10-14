using GameOfLife.Core.Models;
using GameOfLife.Core.Service;
using Xunit;

namespace GameOfLife.Tests
{
    public class GameStateTests
    {
        private readonly BoardService _boardService = new();

        /// Summary:
        /// Checks that calculating a generation does not overwrite the previous board.
        [Fact]
        public void Test_CalculateNextGeneration_PreservesPreviousBoard()
        {
            // Arrange
            var initialBoard = new Board(3, 5);
            _boardService.SetCell(initialBoard, 1, 2, true);
            var game = new GameService(_boardService);

            // Act
            Board nextBoard = game.NextIterationStep(initialBoard);

            // Assert
            Assert.NotSame(initialBoard, nextBoard);
            Assert.True(_boardService.IsAlive(initialBoard, 1, 2));
            Assert.False(_boardService.IsAlive(nextBoard, 1, 2));
        }

        /// Summary:
        /// Checks the smallest supported field and its lack of living neighbours.
        [Fact]
        public void Test_CalculateNextGeneration_HandlesSingleCellBoard()
        {
            // Arrange
            var board = new Board(1, 1);
            _boardService.SetCell(board, 0, 0, true);
            var game = new GameService(_boardService);

            // Act
            Board nextBoard = game.NextIterationStep(board);

            // Assert
            Assert.False(_boardService.IsAlive(nextBoard, 0, 0));
        }

        /// Summary:
        /// Checks that advancing a loaded state increments its counter without changing the old state.
        [Fact]
        public void Test_NextIterationStep_AdvancesStateAndPreservesPreviousState()
        {
            var board = new Board(3, 5);
            _boardService.SetCell(board, 1, 2, true);
            var state = new GameState(board, 12);
            var game = new GameService(_boardService);

            GameState nextState = game.NextIterationStep(state);

            Assert.Equal(12L, state.Iteration);
            Assert.Equal(13L, nextState.Iteration);
            Assert.NotSame(state.Board, nextState.Board);
            Assert.Equal(1, _boardService.CountLivingCells(state.Board));
            Assert.Equal(0, _boardService.CountLivingCells(nextState.Board));
        }

        /// Summary:
        /// Checks that a new game starts at iteration zero.
        [Fact]
        public void Test_GameState_StartsAtZero()
        {
            var state = new GameState(new Board(2, 3));

            Assert.Equal(0L, state.Iteration);
        }
    }
}
