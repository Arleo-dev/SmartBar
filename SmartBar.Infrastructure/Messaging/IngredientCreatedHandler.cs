using Microsoft.Extensions.Logging;
using SmartBar.Application.Ingredients.Events;
using Wolverine.Attributes;

namespace SmartBar.Infrastructure.Messaging;

[WolverineHandler]
public class IngredientCreatedHandler
{
    private readonly ILogger<IngredientCreatedHandler> _logger;

    public IngredientCreatedHandler(ILogger<IngredientCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(IngredientCreatedEvent message)
    {
        _logger.LogInformation("[WOLVERINE CONSUMER] Background task started! Ingredient '{Name}' successfully created with ID: {Id}. Simulating sending a push notification to waiters...",
            message.Name, message.IngredientId);

        await Task.Delay(1000);

        _logger.LogInformation("[WOLVERINE CONSUMER] The background task completed successfully.");
    }
}