namespace GameOfLife.Core.Models
{
    public record BoardRequest
    {
        public int Rows { get; init; }
        public int Columns { get; init; }
    }
}
