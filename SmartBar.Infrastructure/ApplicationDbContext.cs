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
    }
}