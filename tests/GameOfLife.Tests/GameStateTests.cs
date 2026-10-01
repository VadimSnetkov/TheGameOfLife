using GameOfLife.ConsoleApp.Models;
using GameOfLife.ConsoleApp.Service;
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
    }
}
