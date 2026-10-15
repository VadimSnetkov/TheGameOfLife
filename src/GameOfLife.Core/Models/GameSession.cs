namespace GameOfLife.Core.Models
{
    public class GameSession
    {
        public const int MaximumGames = 1000;
        public IReadOnlyList<GameState> Games { get; }

        /// Summary:
        /// Stores an ordered snapshot; a game's number is its position plus one.
        public GameSession(IEnumerable<GameState> games)
        {
            ArgumentNullException.ThrowIfNull(games);
            GameState[] snapshot = games.ToArray();
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(snapshot.Length);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.Length, MaximumGames);
            if (snapshot.Any(game => game is null))
            {
                throw new ArgumentException("A session cannot contain a missing game.", nameof(games));
            }

            Games = Array.AsReadOnly(snapshot);
        }
    }
}
