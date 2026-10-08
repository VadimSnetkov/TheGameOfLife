using FluentValidation;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;
using GameOfLife.Core.Validator;
using Xunit;

namespace GameOfLife.Tests
{
    public class BoardFactoryTests
    {
        private readonly BoardService _boardService = new();

        /// Summary:
        /// Checks the 30 percent threshold and row-column mapping using predictable random values.
        [Fact]
        public void Test_CreateRandom_AppliesAliveProbability()
        {
            // Arrange
            var random = new SequenceRandom(0.0, 0.29, 0.3, 0.99);
            var factory = new BoardFactory(new BoardRequestValidator(), random, _boardService);
            var request = new BoardRequest { Rows = 2, Columns = 2 };

            // Act
            Board board = factory.CreateRandom(request);

            // Assert
            Assert.Equal(2, board.Rows);
            Assert.Equal(2, board.Columns);
            Assert.True(_boardService.IsAlive(board, 0, 0));
            Assert.True(_boardService.IsAlive(board, 0, 1));
            Assert.False(_boardService.IsAlive(board, 1, 0));
            Assert.False(_boardService.IsAlive(board, 1, 1));
        }

        /// Summary:
        /// Checks that the factory runs FluentValidation before allocating the board.
        [Theory]
        [InlineData(0, 5)]
        [InlineData(-1, 5)]
        [InlineData(3, 0)]
        [InlineData(3, -1)]
        [InlineData(101, 5)]
        [InlineData(3, 201)]
        public void Test_CreateRandom_RejectsInvalidRequest(int rows, int columns)
        {
            // Arrange
            var factory = new BoardFactory(new BoardRequestValidator(), new Random(1), _boardService);
            var request = new BoardRequest { Rows = rows, Columns = columns };

            // Act and assert
            Assert.Throws<ValidationException>(() => factory.CreateRandom(request));
        }

        /// Summary:
        /// Checks custom rectangular sizes and the accepted dimension boundaries.
        [Theory]
        [InlineData(1, 1)]
        [InlineData(7, 13)]
        [InlineData(100, 200)]
        public void Test_CreateRandom_AcceptsCustomDimensions(int rows, int columns)
        {
            // Arrange
            var factory = new BoardFactory(new BoardRequestValidator(), new Random(1), _boardService);
            var request = new BoardRequest { Rows = rows, Columns = columns };

            // Act
            Board board = factory.CreateRandom(request);

            // Assert
            Assert.Equal(rows, board.Rows);
            Assert.Equal(columns, board.Columns);
        }
    }
}
