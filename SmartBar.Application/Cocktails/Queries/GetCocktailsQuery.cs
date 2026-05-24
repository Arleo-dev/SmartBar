using System.Data;
using Dapper;
using MediatR;
using SmartBar.Application.Interfaces;

namespace SmartBar.Application.Cocktails.Queries;

public record CocktailIngredientResponse(Guid IngredientId, string Name, decimal Amount, string Unit);

public record CocktailResponse(Guid CocktailId, string Name, string? Description, List<CocktailIngredientResponse> Ingredients);

public record GetCocktailsQuery() : IRequest<IEnumerable<CocktailResponse>>;

public class GetCocktailsQueryHandler : IRequestHandler<GetCocktailsQuery, IEnumerable<CocktailResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetCocktailsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IEnumerable<CocktailResponse>> Handle(GetCocktailsQuery request, CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                c.""CocktailId"", c.""Name"", c.""Description"",
                i.""IngredientId"", i.""Name"",
                ci.""Amount"", ci.""Unit""
            FROM ""Cocktails"" c
            LEFT JOIN ""CocktailIngredients"" ci   ON c.""CocktailId"" = ci.""CocktailId""
            LEFT JOIN ""Ingredients"" i ON ci.""IngredientId"" = i.""IngredientId""";

        var cocktailDictionary = new Dictionary<Guid, CocktailResponse>();

        await connection.QueryAsync<dynamic, dynamic, CocktailResponse>(
            sql,
            (cocktailRow, ingredientRow) =>
            {
                Guid cocktailId = cocktailRow.CocktailId;

                if (!cocktailDictionary.TryGetValue(cocktailId, out var cocktailEntry))
                {
                    cocktailEntry = new CocktailResponse(
                        cocktailId,
                        cocktailRow.Name,
                        cocktailRow.Description,
                        new List<CocktailIngredientResponse>()
                    );
                    cocktailDictionary.Add(cocktailId, cocktailEntry);
                }

                if (ingredientRow != null && ingredientRow.IngredientId != null)
                {
                    var ingredientEntry = new CocktailIngredientResponse(
                        ingredientRow.IngredientId,
                        ingredientRow.Name,
                        (decimal)ingredientRow.Amount,
                        ingredientRow.Unit
                    );

                    cocktailEntry.Ingredients.Add(ingredientEntry);
                }

                return cocktailEntry;
            },
            splitOn: "IngredientId"
        );

        return cocktailDictionary.Values;
    }
}