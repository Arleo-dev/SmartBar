using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBar.Domain.Entities
{
    public class CocktailIngredient
    {
        public Guid CocktailId { get; set; }
        public Cocktail Cocktail { get; set; } = null!;

        public Guid IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;

        public decimal Amount { get; set; } 
        public string Unit { get; set; } = "ml";
    }
}
