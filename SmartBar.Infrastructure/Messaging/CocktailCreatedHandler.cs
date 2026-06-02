using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartBar.Application.Cocktails.Events;
using Wolverine.Attributes;

namespace SmartBar.Infrastructure.Messaging;


[WolverineHandler]
public class CocktailCreatedHandler
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CocktailCreatedHandler> _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private const string CacheKey = "cocktails_list";

    public CocktailCreatedHandler(
        IDistributedCache cache,
        ILogger<CocktailCreatedHandler> logger,
        IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _cache = cache;
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
        _logger.LogInformation("Cocktail with ID {Id} was found in the database.", message.CocktailId);
        var ingredients = cocktail.CocktailIngredients.ToList();

        _logger.LogInformation("Found {Count} ingredients for cocktail '{Name}'", ingredients.Count, cocktail.Name);
        await _cache.RemoveAsync(CacheKey, cancellationToken);
        _logger.LogInformation("Redis cache key '{CacheKey}' successfully invalidated.", CacheKey);
    }
}