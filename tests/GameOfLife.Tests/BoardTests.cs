using GameOfLife.ConsoleApp.Models;
using GameOfLife.ConsoleApp.Service;
using Xunit;

namespace GameOfLife.Tests
{
    public class BoardTests
    {
        private readonly BoardService _boardService = new();

        /// Summary:
        /// Checks that a new rectangular board contains only dead cells.
        [Fact]
        public void Test_Constructor_InitializesDeadCells()
        {
            // Act
            var board = new Board(3, 5);

            // Assert
            Assert.Equal(3, board.Rows);
            Assert.Equal(5, board.Columns);
            Assert.All(Enumerable.Range(0, board.Rows), row =>
                Assert.All(Enumerable.Range(0, board.Columns), column =>
                    Assert.False(_boardService.IsAlive(board, row, column))));
        }

        /// Summary:
        /// Checks that direct board construction still rejects invalid dimensions.
        [Theory]
        [InlineData(0, 5)]
        [InlineData(-1, 5)]
        [InlineData(3, 0)]
        [InlineData(3, -1)]
        public void Test_Constructor_RejectsInvalidDimensions(int rows, int columns)
        {
            // Act and assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new Board(rows, columns));
        }

        /// Summary:
        /// Checks that both reading and writing reject coordinates outside a rectangular board.
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(3, 0)]
        [InlineData(0, -1)]
        [InlineData(0, 5)]
        public void Test_CellAccess_RejectsInvalidCoordinates(int row, int column)
        {
            // Arrange
            var board = new Board(3, 5);

            // Act and assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _boardService.IsAlive(board, row, column));
            Assert.Throws<ArgumentOutOfRangeException>(() => _boardService.SetCell(board, row, column, true));
        }

        /// Summary:
        /// Checks that a valid edge cell can be set alive and then dead without changing other cells.
        [Fact]
        public void Test_SetCell_UpdatesOnlyTheSelectedCell()
        {
            // Arrange
            var board = new Board(3, 5);

            // Act
            _boardService.SetCell(board, 2, 4, true);

            // Assert
            Assert.True(_boardService.IsAlive(board, 2, 4));
            Assert.False(_boardService.IsAlive(board, 0, 0));

            // Act
            _boardService.SetCell(board, 2, 4, false);

            // Assert
            Assert.False(_boardService.IsAlive(board, 2, 4));
        }
    }
}
