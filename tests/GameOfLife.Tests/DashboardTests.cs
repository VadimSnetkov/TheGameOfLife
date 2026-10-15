using GameOfLife.ConsoleApp.Models;
using GameOfLife.ConsoleApp.UI;
using GameOfLife.ConsoleApp.Validator;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;

namespace GameOfLife.Tests
{
    public class DashboardTests
    {
        /// Summary:
        /// Validates arbitrary game choices while rejecting duplicates and out-of-range selections.
        [Theory]
        [InlineData(new int[] { 1 }, true)]
        [InlineData(new int[] { 1, 10, 1000 }, true)]
        [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 1000 }, true)]
        [InlineData(new int[] { }, false)]
        [InlineData(new int[] { 1, 1 }, false)]
        [InlineData(new int[] { 0 }, false)]
        [InlineData(new int[] { 1001 }, false)]
        [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, false)]
        public void Test_ViewSelection_ValidatesNumbers(int[] ids, bool expected)
        {
            var validator = new ViewSelectionValidator();
            var selection = new ViewSelection { GameCount = 1000, GameIds = ids };
            Assert.Equal(expected, validator.Validate(selection).IsValid);
        }

        /// Summary:
        /// Fits eight selected views on a standard terminal and includes games outside the first eight.
        [Theory]
        [InlineData(79, 23)]
        [InlineData(119, 39)]
        public void Test_Dashboard_ShowsEightSelectedGamesWithoutOverflow(int width, int height)
        {
            var session = new GameSession(Enumerable.Range(0, 1000)
                .Select(_ => new GameState(new Board(10, 20), 7)));
            int[] ids = { 1, 2, 3, 4, 5, 6, 999, 1000 };
            var renderer = new ConsoleRenderer(new BoardService());
            string[] frame = renderer.BuildFrame(session, new SessionStatistics(1000, 0, 0),
                ids, "Ready", null, width, height);

            Assert.Equal(height, frame.Length);
            Assert.All(frame, line => Assert.Equal(width, line.Length));
            string text = string.Join("\n", frame);
            Assert.All(ids, id => Assert.Contains($"Game {id} | Gen 7", text));
            Assert.Contains("Games: 1000 | Live games: 0 | Living cells: 0", text);
            Assert.DoesNotContain("Game 8 |", text);
        }
    }
}
