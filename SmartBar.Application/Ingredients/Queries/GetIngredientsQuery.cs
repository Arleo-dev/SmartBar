using System.Data;
using System.Text.Json;
using Dapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using SmartBar.Application.Interfaces;

namespace SmartBar.Application.Ingredients.Queries;

public record IngredientResponse(Guid IngredientId, string Name, string? Category);
public record GetIngredientsQuery() : IRequest<IEnumerable<IngredientResponse>>;

public class GetIngredientsQueryHandler : IRequestHandler<GetIngredientsQuery, IEnumerable<IngredientResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IDistributedCache _cache;

    public GetIngredientsQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IDistributedCache cache)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _cache = cache;
    }

    public async Task<IEnumerable<IngredientResponse>> Handle(GetIngredientsQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = "ingredients_list";

        string? cachedIngredients = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedIngredients))
        {
            return JsonSerializer.Deserialize<IEnumerable<IngredientResponse>>(cachedIngredients)!;
        }

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();
        const string sql = "SELECT \"IngredientId\", \"Name\", \"Category\" FROM \"Ingredients\"";
        var ingredients = (await connection.QueryAsync<IngredientResponse>(sql)).ToList();

        if (ingredients.Any())
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            string jsonString = JsonSerializer.Serialize(ingredients);
            await _cache.SetStringAsync(cacheKey, jsonString, cacheOptions, cancellationToken);
        }

        return ingredients;
    }
}