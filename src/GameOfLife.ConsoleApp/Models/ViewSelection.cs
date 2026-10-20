namespace GameOfLife.ConsoleApp.Models
{
    public record ViewSelection
    {
        public const int MaximumVisibleGames = 8;
        public required int[] GameIds { get; init; }
        public int GameCount { get; init; }
    }
}
