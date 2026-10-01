namespace GameOfLife.ConsoleApp.Models
{
    public record MenuRequest
    {
        public MenuChoice Choice { get; init; }
    }
}
