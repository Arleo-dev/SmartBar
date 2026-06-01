namespace SmartBar.Domain.Entities
{
    public class Cocktail
    {
        public Guid CocktailId { get; private set; } = Guid.CreateVersion7();
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RecipeSteps { get; set; } = string.Empty;
        public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
    }
}
