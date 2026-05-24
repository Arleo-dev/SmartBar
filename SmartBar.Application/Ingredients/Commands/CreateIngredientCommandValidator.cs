using FluentValidation;

namespace SmartBar.Application.Ingredients.Commands;
public class CreateIngredientCommandValidator : AbstractValidator<CreateIngredientCommand>
{
    public CreateIngredientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ingredient name cannot be empty")
            .MinimumLength(3).WithMessage("The name must be longer than 3 characters.")
            .MaximumLength(50).WithMessage("The name cannot be longer than 50 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(30).WithMessage("The сategory cannot be longer than 30 characters.")
            .When(x => !string.IsNullOrEmpty(x.Category)); 
    }
}