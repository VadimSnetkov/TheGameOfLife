using FluentValidation;
using GameOfLife.Core.Models;

namespace GameOfLife.Core.Validator
{
    public class GameSaveDataValidator : AbstractValidator<GameSaveData>
    {
        /// Summary:
        /// Checks saved dimensions, iteration count, and cell data before rebuilding a board.
        public GameSaveDataValidator(IValidator<BoardRequest> boardValidator)
        {
            ArgumentNullException.ThrowIfNull(boardValidator);

            RuleFor(data => new BoardRequest { Rows = data.Rows, Columns = data.Columns })
                .SetValidator(boardValidator);

            RuleFor(data => data.Iteration)
                .GreaterThanOrEqualTo(0)
                .LessThan(long.MaxValue)
                .WithMessage("The saved iteration count is outside the supported range.");

            RuleFor(data => data.Cells)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .Must((data, cells) => cells.LongLength == (long)data.Rows * data.Columns)
                .WithMessage("The saved cell count must match the board dimensions.");
        }
    }
}
