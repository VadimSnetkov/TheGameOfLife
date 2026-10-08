namespace GameOfLife.Core.Models
{
    public class GameSaveData
    {
        public required int Rows { get; init; }
        public required int Columns { get; init; }
        public required long Iteration { get; init; }
        public required bool[] Cells { get; init; }
    }
}
