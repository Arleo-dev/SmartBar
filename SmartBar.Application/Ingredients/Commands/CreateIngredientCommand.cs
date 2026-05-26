using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using SmartBar.Application.Ingredients.Events;
using SmartBar.Application.Interfaces;
using SmartBar.Domain.Entities;
using Wolverine; // 👈 Замість MassTransit

namespace SmartBar.Application.Ingredients.Commands;

public record CreateIngredientCommand(string Name, string? Category) : IRequest<Guid>;

public class CreateIngredientCommandHandler : IRequestHandler<CreateIngredientCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly IMessageBus _bus;

    public CreateIngredientCommandHandler(
        IApplicationDbContext context,
        IDistributedCache cache,
        IMessageBus bus)
    {
        _context = context;
        _cache = cache;
        _bus = bus;
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

        await _bus.PublishAsync(new IngredientCreatedEvent(ingredient.IngredientId, ingredient.Name));

        return ingredient.IngredientId;
    }
}