using FluentValidation;
using GameOfLife.Core.Models;

namespace GameOfLife.Core.Validator
{
    public class SessionRequestValidator : AbstractValidator<SessionRequest>
    {
        /// Summary:
        /// Validates the common board size and the supported number of games.
        public SessionRequestValidator(IValidator<BoardRequest> boardValidator)
        {
            RuleFor(request => request.Board).NotNull().SetValidator(boardValidator);
            RuleFor(request => request.GameCount).InclusiveBetween(1, GameSession.MaximumGames);
        }
    }
}
