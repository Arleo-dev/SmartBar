using MediatR;
using MassTransit; 
using SmartBar.Domain.Entities;
using SmartBar.Application.Interfaces;
using SmartBar.Application.Ingredients.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace SmartBar.Application.Ingredients.Commands;

public record CreateIngredientCommand(string Name, string? Category) : IRequest<Guid>;

public class CreateIngredientCommandHandler : IRequestHandler<CreateIngredientCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateIngredientCommandHandler(
        IApplicationDbContext context,
        IDistributedCache cache,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _cache = cache;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category
        };

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync("ingredients_list", cancellationToken);
        await _publishEndpoint.Publish(new IngredientCreatedEvent(ingredient.IngredientId, ingredient.Name), cancellationToken);

        return ingredient.IngredientId;
    }
}