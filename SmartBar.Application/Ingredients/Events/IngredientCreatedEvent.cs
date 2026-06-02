using Wolverine.Attributes;

namespace SmartBar.Application.Ingredients.Events
{
    [MessageIdentity("ingredient-created-queue")]
    public record IngredientCreatedEvent(Guid IngredientId, string Name);
}