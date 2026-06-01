namespace SmartBar.Domain.Entities
{
    public class Ingredient
    {
        public Guid IngredientId { get; private set; } = Guid.CreateVersion7();
        public required string Name { get; set; }
        public string? Category { get; set; }
        public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
    }
}
