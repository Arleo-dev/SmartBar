using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace SmartBar.Application.Cocktails.Queries;

public class CachedGetCocktailsQueryHandler : IRequestHandler<GetCocktailsQuery, IEnumerable<CocktailResponse>>
{
    private readonly GetCocktailsQueryHandler _innerHandler;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "cocktails_list";

    public CachedGetCocktailsQueryHandler(
        GetCocktailsQueryHandler innerHandler,
        IDistributedCache cache)
    {
        _innerHandler = innerHandler;
        _cache = cache;
    }

    public async Task<IEnumerable<CocktailResponse>> Handle(GetCocktailsQuery request, CancellationToken cancellationToken)
    {
        string? cachedCocktails = await _cache.GetStringAsync(CacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedCocktails))
        {
            return JsonSerializer.Deserialize<IEnumerable<CocktailResponse>>(cachedCocktails)!;
        }

        var cocktails = await _innerHandler.Handle(request, cancellationToken);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        string serializedCocktails = JsonSerializer.Serialize(cocktails);
        await _cache.SetStringAsync(CacheKey, serializedCocktails, cacheOptions, cancellationToken);

        return cocktails;
    }
}