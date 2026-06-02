using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartBar.Application.Cocktails.Events;
using Wolverine.Attributes;

namespace SmartBar.Infrastructure.Messaging
{
    [WolverineHandler]
    public class CocktailOrderedHandler
    {
        private readonly ILogger<CocktailOrderedHandler> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CocktailOrderedHandler(
            ILogger<CocktailOrderedHandler> logger,
            IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
        }

        public async Task Handle(CocktailOrderedEvent message, CancellationToken cancellationToken)
        {
            var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var cocktail = await context.Cocktails
                .Include(c => c.CocktailIngredients)
                .ThenInclude(ci => ci.Ingredient)
                .FirstOrDefaultAsync(x => x.CocktailId == message.CocktailId, cancellationToken);

            if (cocktail == null)
            {
                _logger.LogWarning("Cocktail with ID {Id} was not found in the database.", message.CocktailId);
                return;
            }

            var cocktailIngredients = cocktail.CocktailIngredients.ToList();
            if (cocktailIngredients.Any())
            {
                var ingredientList = string.Join(", ", cocktailIngredients.Select(ci => ci.Ingredient?.Name ?? "Unknown"));
                _logger.LogInformation("Cocktail with ID {Id} was ordered. Ingredients: {Ingredients}", message.CocktailId, ingredientList);

                var ingredientIds = cocktailIngredients.Select(ci => ci.IngredientId).ToList();
                var inventories = await context.Inventories
                    .Where(i => ingredientIds.Contains(i.IngredientId))
                    .ToListAsync(cancellationToken);

                foreach (var ci in cocktailIngredients)
                {
                    var inventory = inventories.FirstOrDefault(i => i.IngredientId == ci.IngredientId);

                    if (inventory == null)
                    {
                        throw new InvalidOperationException($"Ingredient with ID {ci.IngredientId} is not available in inventory.");
                    }

                    inventory.DecreaseStock(ci.Amount * message.Quantity);
                }

                await context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("Cocktail with ID {Id} was ordered, but it has no ingredients.", message.CocktailId);
            }
        }
    }
}
