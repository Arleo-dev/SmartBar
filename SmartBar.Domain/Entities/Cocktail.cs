using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBar.Domain.Entities
{
    public class Cocktail
    {
        public required Guid CocktailId { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RecipeSteps { get; set; } = string.Empty;
        public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
    }
}
