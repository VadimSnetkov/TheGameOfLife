using FluentValidation;
using GameOfLife.ConsoleApp.Models;

namespace GameOfLife.ConsoleApp.Validator
{
    public class MenuRequestValidator : AbstractValidator<MenuRequest>
    {
        /// Summary:
        /// Accepts only the choices declared by the menu enum.
        public MenuRequestValidator()
        {
            RuleFor(x => x.Choice)
                .IsInEnum()
                .WithMessage("Choose one of the displayed menu options.");
        }
    }
}
