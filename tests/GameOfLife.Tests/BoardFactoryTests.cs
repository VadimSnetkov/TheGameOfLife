using FluentValidation;
using GameOfLife.ConsoleApp.Models;
using GameOfLife.ConsoleApp.Service;
using GameOfLife.ConsoleApp.Validator;
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
        [Fact]
        public void Test_CreateRandom_RejectsInvalidRequest()
        {
            // Arrange
            var factory = new BoardFactory(new BoardRequestValidator(), new Random(1), _boardService);
            var request = new BoardRequest { Rows = 0, Columns = 5 };

            // Act and assert
            Assert.Throws<ValidationException>(() => factory.CreateRandom(request));
        }

        private sealed class SequenceRandom : Random
        {
            private readonly Queue<double> _values;

            /// Summary:
            /// Supplies a fixed sequence so the test does not depend on chance.
            public SequenceRandom(params double[] values)
            {
                _values = new Queue<double>(values);
            }

            /// Summary:
            /// Returns the next predefined random value.
            public override double NextDouble()
            {
                return _values.Dequeue();
            }
        }
    }
}
