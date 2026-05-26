using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartBar.Application.Cocktails.Events;
using SmartBar.Application.Interfaces;
using SmartBar.Domain.Entities;
using Wolverine; // 👈 Замість MassTransit

namespace SmartBar.Application.Cocktails.Commands;

public record CocktailIngredientDto(Guid IngredientId, decimal Amount, string Unit);

public record CreateCocktailCommand(
    string Name,
    string? Description,
    List<CocktailIngredientDto> Ingredients) : IRequest<Guid>;

public class CreateCocktailCommandHandler : IRequestHandler<CreateCocktailCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IMessageBus _bus;

    public CreateCocktailCommandHandler(IApplicationDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    public async Task<Guid> Handle(CreateCocktailCommand request, CancellationToken cancellationToken)
    {
        var requestIngredientIds = request.Ingredients.Select(i => i.IngredientId).ToList();

        var existingIngredientsCount = await _context.Ingredients
            .Where(i => requestIngredientIds.Contains(i.IngredientId))
            .CountAsync(cancellationToken);

        if (existingIngredientsCount != requestIngredientIds.Count)
        {
            throw new InvalidOperationException("One or more ingredients were not found.");
        }

        var cocktail = new Cocktail
        {
            CocktailId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        cocktail.CocktailIngredients = request.Ingredients.Select(dto => new CocktailIngredient
        {
            CocktailId = cocktail.CocktailId,
            IngredientId = dto.IngredientId,
            Amount = dto.Amount,
            Unit = dto.Unit
        }).ToList();

        _context.Cocktails.Add(cocktail);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        await _bus.PublishAsync(new CocktailCreatedEvent(cocktail.CocktailId, request.Name));

        return cocktail.CocktailId;
    }
}