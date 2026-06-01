using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartBar.Application.Ingredients.Events;
using SmartBar.Domain.Entities;
using Wolverine.Attributes;

namespace SmartBar.Infrastructure.Messaging;

[WolverineHandler]
public class IngredientCreatedHandler
{
    private readonly ILogger<IngredientCreatedHandler> _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public IngredientCreatedHandler(ILogger<IngredientCreatedHandler> logger,
            IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async Task Handle(IngredientCreatedEvent message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[WOLVERINE CONSUMER] Background task started! Ingredient '{Name}' successfully created with ID: {Id}. Simulating sending a push notification to waiters...",
            message.Name, message.IngredientId);
        using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var inventory = await context.Inventories.FirstOrDefaultAsync(i => i.IngredientId == message.IngredientId, cancellationToken);
        if (inventory == null)
        {
            //TODO: change category to enum and take Unit from there
            context.Inventories.Add(new Inventory(message.IngredientId, 0, "ml"));
            await context.SaveChangesAsync(cancellationToken);
        }
        _logger.LogInformation("[WOLVERINE CONSUMER] The background task completed successfully.");
    }
}