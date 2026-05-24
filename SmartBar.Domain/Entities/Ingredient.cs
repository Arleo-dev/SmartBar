using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBar.Domain.Entities
{
    public class Ingredient
    {
        public required Guid IngredientId { get; set; }
        public required string Name { get; set; }
        public string? Category { get; set; }
        public ICollection<CocktailIngredient> CocktailIngredients { get; set; } = new List<CocktailIngredient>();
    }
}
