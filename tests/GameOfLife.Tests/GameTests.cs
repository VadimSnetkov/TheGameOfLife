using GameOfLife.ConsoleApp.Models;
using Xunit;

namespace GameOfLife.Tests
{
    public class GameTests
    {
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
        public void Advance_AppliesRulesToCenterCell(
            bool initiallyAlive,
            int livingNeighbours,
            bool expectedAlive)
        {
            var board = new Board(3, 3);
            board.SetCell(1, 1, initiallyAlive);

            var neighbours = new (int Row, int Column)[]
            {
                (0, 0), (0, 1), (0, 2),
                (1, 0),         (1, 2),
                (2, 0), (2, 1), (2, 2)
            };

            for (int index = 0; index < livingNeighbours; index++)
            {
                var neighbour = neighbours[index];
                board.SetCell(neighbour.Row, neighbour.Column, true);
            }

            var game = new Game(board);

            game.CalculateNextGeneration();

            Assert.Equal(expectedAlive, game.CurrentBoard.IsAlive(1, 1));
        }

        [Fact]
        public void Advance_BlinkerReturnsAfterTwoGenerations()
        {
            var board = new Board(5, 5);
            board.SetCell(2, 1, true);
            board.SetCell(2, 2, true);
            board.SetCell(2, 3, true);

            var game = new Game(board);

            game.CalculateNextGeneration();

            AssertLivingCells(game.CurrentBoard, (1, 2), (2, 2), (3, 2));

            game.CalculateNextGeneration();

            AssertLivingCells(game.CurrentBoard, (2, 1), (2, 2), (2, 3));
        }

        [Fact]
        public void Advance_CornerCellsFormStableBlockWithoutWrapping()
        {
            var board = new Board(3, 5);
            board.SetCell(0, 0, true);
            board.SetCell(0, 1, true);
            board.SetCell(1, 0, true);

            var game = new Game(board);

            game.CalculateNextGeneration();

            AssertLivingCells(
                game.CurrentBoard,
                (0, 0), (0, 1), (1, 0), (1, 1));

            game.CalculateNextGeneration();

            AssertLivingCells(
                game.CurrentBoard,
                (0, 0), (0, 1), (1, 0), (1, 1));
        }

        private static void AssertLivingCells(
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
                        board.IsAlive(row, column));
                }
            }
        }
    }
}
