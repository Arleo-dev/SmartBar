using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartBar.Application.Cocktails.Events;
using SmartBar.Application.Interfaces;
using Wolverine;
using Wolverine.Attributes;

namespace SmartBar.Infrastructure.Messaging;

[WolverineHandler]
public class CocktailCreatedHandler
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CocktailCreatedHandler> _logger;
    private const string CacheKey = "cocktails_list";

    public CocktailCreatedHandler(IDistributedCache cache, ILogger<CocktailCreatedHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(CocktailCreatedEvent message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing unique event via RabbitMQ. Cocktail: {CocktailName}", message.Name);

        await _cache.RemoveAsync(CacheKey, cancellationToken);

        _logger.LogInformation("Redis cache key '{CacheKey}' successfully invalidated.", CacheKey);
    }
}