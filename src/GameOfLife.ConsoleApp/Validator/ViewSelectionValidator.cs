using FluentValidation;
using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.Validator
{
    public class ViewSelectionValidator : AbstractValidator<ViewSelection>
    {
        /// Summary:
        /// Accepts up to eight distinct game numbers that exist in the current session.
        public ViewSelectionValidator()
        {
            RuleFor(selection => selection.GameIds)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .Must(ids => ids.Length is >= 1 and <= ViewSelection.MaximumVisibleGames)
                .WithMessage("Choose between 1 and 8 games.")
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .WithMessage("Choose each game only once.")
                .Must((selection, ids) => ids.All(id => id >= 1 && id <= selection.GameCount))
                .WithMessage("Every game number must exist in the session.");
        }
    }
}
