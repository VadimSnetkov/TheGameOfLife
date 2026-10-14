using GameOfLife.Core.Models;
using GameOfLife.Core.Service;
using Xunit;

namespace GameOfLife.Tests
{
    public class GameTests
    {
        private readonly BoardService _boardService = new();

        /// Summary:
        /// Checks all eighteen combinations of cell state and living-neighbour count.
        [Theory]
        [InlineData(false, 0, false)]
        [InlineData(false, 1, false)]
        [InlineData(false, 2, false)]
        [InlineData(false, 3, true)]
        [InlineData(false, 4, false)]
        [InlineData(false, 5, false)]
        [InlineData(false, 6, false)]
        [InlineData(false, 7, false)]
        [InlineData(false, 8, false)]
        [InlineData(true, 0, false)]
        [InlineData(true, 1, false)]
        [InlineData(true, 2, true)]
        [InlineData(true, 3, true)]
        [InlineData(true, 4, false)]
        [InlineData(true, 5, false)]
        [InlineData(true, 6, false)]
        [InlineData(true, 7, false)]
        [InlineData(true, 8, false)]
        public void Test_CalculateNextGeneration_AppliesRulesToCenterCell(
            bool initiallyAlive,
            int livingNeighbours,
            bool expectedAlive)
        {
            // Arrange
            var board = new Board(3, 3);
            _boardService.SetCell(board, 1, 1, initiallyAlive);

            var neighbours = new (int Row, int Column)[]
            {
                (0, 0), (0, 1), (0, 2),
                (1, 0),         (1, 2),
                (2, 0), (2, 1), (2, 2)
            };

            for (int index = 0; index < livingNeighbours; index++)
            {
                var neighbour = neighbours[index];
                _boardService.SetCell(board, neighbour.Row, neighbour.Column, true);
            }

            var game = new GameService(_boardService);

            // Act
            board = game.NextIterationStep(board);

            // Assert
            Assert.Equal(expectedAlive, _boardService.IsAlive(board, 1, 1));
        }

        /// Summary:
        /// Checks that a blinker oscillates and returns to its initial state.
        [Fact]
        public void Test_CalculateNextGeneration_BlinkerReturnsAfterTwoGenerations()
        {
            // Arrange
            var board = new Board(5, 5);
            _boardService.SetCell(board, 2, 1, true);
            _boardService.SetCell(board, 2, 2, true);
            _boardService.SetCell(board, 2, 3, true);

            var game = new GameService(_boardService);

            // Act
            board = game.NextIterationStep(board);

            // Assert
            AssertLivingCells(board, (1, 2), (2, 2), (3, 2));

            // Act
            board = game.NextIterationStep(board);

            // Assert
            AssertLivingCells(board, (2, 1), (2, 2), (2, 3));
        }

        /// Summary:
        /// Checks finite boundaries on a rectangular board and stable-block survival.
        [Fact]
        public void Test_CalculateNextGeneration_CornerCellsFormStableBlockWithoutWrapping()
        {
            // Arrange
            var board = new Board(3, 5);
            _boardService.SetCell(board, 0, 0, true);
            _boardService.SetCell(board, 0, 1, true);
            _boardService.SetCell(board, 1, 0, true);

            var game = new GameService(_boardService);

            // Act
            board = game.NextIterationStep(board);

            // Assert
            AssertLivingCells(
                board,
                (0, 0), (0, 1), (1, 0), (1, 1));

            // Act
            board = game.NextIterationStep(board);

            // Assert
            AssertLivingCells(
                board,
                (0, 0), (0, 1), (1, 0), (1, 1));
        }

        /// Summary:
        /// Checks every board cell against the expected living coordinates.
        private void AssertLivingCells(
            Board board,
            params (int Row, int Column)[] expectedCells)
        {
            var expected = new HashSet<(int Row, int Column)>(expectedCells);

            for (int row = 0; row < board.Rows; row++)
            {
                for (int column = 0; column < board.Columns; column++)
                {
                    Assert.Equal(
                        expected.Contains((row, column)),
                        _boardService.IsAlive(board, row, column));
                }
            }
        }
    }
}
