using FluentValidation;
using GameOfLife.Core.Models;

namespace GameOfLife.Core.Service
{
    public class BoardFactory
    {
        private const double InitialAliveProbability = 0.3;
        private readonly IValidator<BoardRequest> _validator;
        private readonly Random _random;
        private readonly BoardService _boardService;

        /// Summary:
        /// Receives the validator, random generator, and service used to fill boards.
        public BoardFactory(IValidator<BoardRequest> validator, Random random, BoardService boardService)
        {
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(random);
            ArgumentNullException.ThrowIfNull(boardService);

            _validator = validator;
            _random = random;
            _boardService = boardService;
        }

        /// Summary:
        /// Validates the requested size and gives each cell a 30 percent chance of being alive.
        public Board CreateRandom(BoardRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            _validator.ValidateAndThrow(request);

            var board = new Board(request.Rows, request.Columns);
            for (int row = 0; row < board.Rows; row++)
            {
                for (int column = 0; column < board.Columns; column++)
                {
                    bool isAlive = _random.NextDouble() < InitialAliveProbability;
                    _boardService.SetCell(board, row, column, isAlive);
                }
            }

            return board;
        }
    }
}
