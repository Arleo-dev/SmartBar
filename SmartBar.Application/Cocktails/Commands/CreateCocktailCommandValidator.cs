using FluentValidation;

namespace SmartBar.Application.Cocktails.Commands;

public class CreateCocktailCommandValidator : AbstractValidator<CreateCocktailCommand>
{
    public CreateCocktailCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cocktail name is required.")
            .MaximumLength(100).WithMessage("Cocktail name must not exceed 100 characters.");

        RuleFor(x => x.Ingredients)
            .NotEmpty().WithMessage("Cocktail must contain at least one ingredient.");

        RuleForEach(x => x.Ingredients).ChildRules(ingredient =>
        {
            ingredient.RuleFor(i => i.IngredientId)
                .NotEmpty().WithMessage("Ingredient ID is required.");

            ingredient.RuleFor(i => i.Amount)
                .GreaterThan(0).WithMessage("Ingredient amount must be greater than 0.");

            ingredient.RuleFor(i => i.Unit)
                .NotEmpty().WithMessage("Measurement unit is required.")
                .MaximumLength(10).WithMessage("Measurement unit must not exceed 10 characters.");
        });
    }
}