using Microsoft.EntityFrameworkCore;
using SmartBar.Domain.Entities;

namespace SmartBar.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Ingredient> Ingredients { get; }
        DbSet<Cocktail> Cocktails { get; }
        DbSet<CocktailIngredient> CocktailIngredients { get; }
        DbSet<InboxMessage> InboxMessages { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
