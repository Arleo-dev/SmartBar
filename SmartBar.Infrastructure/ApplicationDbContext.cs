using Microsoft.EntityFrameworkCore;
using SmartBar.Application.Interfaces;
using SmartBar.Domain.Entities;

namespace SmartBar.Infrastructure;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Cocktail> Cocktails => Set<Cocktail>();
    public DbSet<CocktailIngredient> CocktailIngredients => Set<CocktailIngredient>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(i => i.IngredientId);
            entity.HasIndex(i => i.Name).IsUnique();
        });

        modelBuilder.Entity<Cocktail>(entity =>
        {
            entity.HasKey(c => c.CocktailId);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<CocktailIngredient>(entity =>
        {
            entity.HasKey(ci => new { ci.CocktailId, ci.IngredientId });

            entity.HasOne(ci => ci.Cocktail)
                .WithMany(c => c.CocktailIngredients)
                .HasForeignKey(ci => ci.CocktailId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.Ingredient)
                .WithMany(i => i.CocktailIngredients)
                .HasForeignKey(ci => ci.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(ci => ci.Amount).HasPrecision(18, 2);
            entity.Property(ci => ci.Unit).HasMaxLength(10);
        });

#if DEBUG
        var bourbonId = Guid.Parse("b1111111-1111-1111-1111-111111111111");
        var vermouthId = Guid.Parse("c2222222-2222-2222-2222-222222222222");
        var bittersId = Guid.Parse("b3333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Ingredient>().HasData(
            new Ingredient { IngredientId = bourbonId, Name = "Bourbon", Category = "Whiskey" },
            new Ingredient { IngredientId = vermouthId, Name = "Sweet Vermouth", Category = "Vermouth" },
            new Ingredient { IngredientId = bittersId, Name = "Angostura Bitters", Category = "Bitters" }
        );

        var manhattanId = Guid.Parse("a4444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Cocktail>().HasData(
            new Cocktail
            {
                CocktailId = manhattanId,
                Name = "Manhattan",
                Description = "Classic cocktail made with rye or bourbon, sweet vermouth, and bitters."
            }
        );

        modelBuilder.Entity<CocktailIngredient>().HasData(
            new CocktailIngredient { CocktailId = manhattanId, IngredientId = bourbonId, Amount = 60, Unit = "ml" },
            new CocktailIngredient { CocktailId = manhattanId, IngredientId = vermouthId, Amount = 30, Unit = "ml" },
            new CocktailIngredient { CocktailId = manhattanId, IngredientId = bittersId, Amount = 2, Unit = "dashes" }
        );
#endif
    }
}