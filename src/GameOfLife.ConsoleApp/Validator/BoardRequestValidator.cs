using FluentValidation;
using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.Validator
{
    public class BoardRequestValidator : AbstractValidator<BoardRequest>
    {
        /// Summary:
        /// Requires positive dimensions before creating a board.
        public BoardRequestValidator()
        {
            RuleFor(x => x.Rows)
                .GreaterThan(0)
                .WithMessage("Rows must be greater than zero.");

            RuleFor(x => x.Columns)
                .GreaterThan(0)
                .WithMessage("Columns must be greater than zero.");
        }
    }
}
