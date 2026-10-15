using FluentValidation;
using GameOfLife.Core.Models;

namespace GameOfLife.Core.Service
{
    public class SessionService
    {
        private readonly BoardFactory _boardFactory;
        private readonly GameService _gameService;
        private readonly BoardService _boardService;
        private readonly IValidator<SessionRequest> _validator;

        /// Summary:
        /// Receives board creation, evolution, cell counting, and request validation.
        public SessionService(BoardFactory boardFactory, GameService gameService,
            BoardService boardService, IValidator<SessionRequest> validator)
        {
            ArgumentNullException.ThrowIfNull(boardFactory);
            ArgumentNullException.ThrowIfNull(gameService);
            ArgumentNullException.ThrowIfNull(boardService);
            ArgumentNullException.ThrowIfNull(validator);
            _boardFactory = boardFactory;
            _gameService = gameService;
            _boardService = boardService;
            _validator = validator;
        }

        /// Summary:
        /// Creates independent boards with stable game numbers from one to the requested count.
        public GameSession Create(SessionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            _validator.ValidateAndThrow(request);
            var games = new GameState[request.GameCount];
            for (int index = 0; index < games.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                games[index] = new GameState(_boardFactory.CreateRandom(request.Board));
            }

            return new GameSession(games);
        }

        /// Summary:
        /// Advances all games in parallel and publishes a new session only after every game finishes.
        public GameSession NextIterationStep(GameSession current, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(current);
            var next = new GameState[current.Games.Count];
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.For(0, next.Length, options, index =>
            {
                next[index] = _gameService.NextIterationStep(current.Games[index], cancellationToken);
            });

            cancellationToken.ThrowIfCancellationRequested();
            return new GameSession(next);
        }

        /// Summary:
        /// Counts all games containing living cells and sums the population across the whole session.
        public SessionStatistics GetStatistics(GameSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            int liveGames = 0;
            long livingCells = 0;
            foreach (GameState game in session.Games)
            {
                int count = _boardService.CountLivingCells(game.Board);
                if (count > 0)
                {
                    liveGames++;
                }
                livingCells += count;
            }

            return new SessionStatistics(session.Games.Count, liveGames, livingCells);
        }
    }
}
