using FluentValidation;
using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.Validator
{
    public class BoardRequestValidator : AbstractValidator<BoardRequest>
    {
        private const int MaximumRows = 100;
        private const int MaximumColumns = 200;

        /// Summary:
        /// Limits board dimensions to a practical size for the console application.
        public BoardRequestValidator()
        {
            RuleFor(x => x.Rows)
                .InclusiveBetween(1, MaximumRows)
                .WithMessage($"Rows must be between 1 and {MaximumRows}.");

            RuleFor(x => x.Columns)
                .InclusiveBetween(1, MaximumColumns)
                .WithMessage($"Columns must be between 1 and {MaximumColumns}.");
        }
    }
}
