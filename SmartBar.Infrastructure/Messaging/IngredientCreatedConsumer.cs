using MassTransit;
using Microsoft.Extensions.Logging;
using SmartBar.Application.Ingredients.Events;

namespace SmartBar.Infrastructure.Messaging;

public class IngredientCreatedConsumer : IConsumer<IngredientCreatedEvent>
{
    private readonly ILogger<IngredientCreatedConsumer> _logger;

    public IngredientCreatedConsumer(ILogger<IngredientCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IngredientCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("[RABBITMQ CONSUMER] Background task started! Ingredient '{Name}' successfully created with ID: {Id}. Simulating sending a push notification to waiters...",
            message.Name, message.IngredientId);

        await Task.Delay(1000);

        _logger.LogInformation("[RABBITMQ CONSUMER] The background task completed successfully.");
    }
}