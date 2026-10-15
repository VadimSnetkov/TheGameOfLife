using FluentValidation;
using GameOfLife.Core.Models;

namespace GameOfLife.Core.Validator
{
    public class SessionSaveDataValidator : AbstractValidator<SessionSaveData>
    {
        /// Summary:
        /// Validates the file version, game count, and every saved game.
        public SessionSaveDataValidator(IValidator<GameSaveData> gameValidator)
        {
            RuleFor(data => data.Version).Equal(1);
            RuleFor(data => data.Games)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .Must(games => games.Length is >= 1 and <= GameSession.MaximumGames)
                .WithMessage($"A save must contain between 1 and {GameSession.MaximumGames} games.")
                .DependentRules(() =>
                {
                    RuleForEach(data => data.Games).NotNull().SetValidator(gameValidator);
                });
        }
    }
}
