namespace GameOfLife.Core.Models
{
    public class SessionSaveData
    {
        public required int Version { get; init; }
        public required GameSaveData[] Games { get; init; }
    }
}
