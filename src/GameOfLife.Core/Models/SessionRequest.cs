namespace GameOfLife.Core.Models
{
    public record SessionRequest
    {
        public BoardRequest Board { get; init; } = new();
        public int GameCount { get; init; }
    }
}
