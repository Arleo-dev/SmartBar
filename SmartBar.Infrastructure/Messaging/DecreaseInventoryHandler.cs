using JasperFx.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartBar.Application.Cocktails.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine.Attributes;

namespace SmartBar.Infrastructure.Messaging
{
    [WolverineHandler]
    public class DecreaseInventoryHandler
    {
        private readonly ILogger<CocktailCreatedHandler> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DecreaseInventoryHandler(
            ILogger<CocktailCreatedHandler> logger,
            IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
        }

        public async Task Handle(CocktailCreatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing unique event via RabbitMQ. Cocktail: {CocktailName}", message.Name);

            using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var cocktail = await context.Cocktails
                .Include(c => c.CocktailIngredients)
                .ThenInclude(ci => ci.Ingredient)
                .FirstOrDefaultAsync(x => x.CocktailId == message.CocktailId, cancellationToken);

            if (cocktail == null)
            {
                _logger.LogWarning("Cocktail with ID {Id} was not found in the database.", message.CocktailId);
                return;
            }
            var ingredients = cocktail.CocktailIngredients.ToList();
            context.Inventories.Where(i => ingredients.Select(ci => ci.IngredientId).Contains(i.IngredientId))
                .ToList()
                .ForEach(i =>
                {
                    var cocktailIngredient = ingredients.First(ci => ci.IngredientId == i.IngredientId);
                    i.DecreaseStock(cocktailIngredient.Amount);
                    _logger.LogInformation("Cocktail Ingredient with ID {Id} was be Decrease", cocktailIngredient.IngredientId);
                });
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
