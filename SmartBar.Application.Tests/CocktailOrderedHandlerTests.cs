using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartBar.Application.Cocktails.Events;
using SmartBar.Domain.Entities;
using SmartBar.Infrastructure;
using SmartBar.Infrastructure.Messaging;

namespace SmartBar.Application.Tests
{
    public class CocktailOrderedHandlerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        private readonly IDbContextFactory<ApplicationDbContext> _factoryMock;
        private readonly ILogger<CocktailOrderedHandler> _loggerMock;
        private readonly CocktailOrderedHandler _handler;

        public CocktailOrderedHandlerTests()
        {
            SQLitePCL.Batteries.Init();
            var uniqueDbName = Guid.NewGuid().ToString();
            _connection = new SqliteConnection($"Data Source=file:{uniqueDbName}?mode=memory&cache=private");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            using (var context = new ApplicationDbContext(_contextOptions))
            {
                context.Database.EnsureCreated();
            }

            _factoryMock = Substitute.For<IDbContextFactory<ApplicationDbContext>>();
            _factoryMock.CreateDbContextAsync(Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromResult(new ApplicationDbContext(_contextOptions)));

            _loggerMock = Substitute.For<ILogger<CocktailOrderedHandler>>();
            _handler = new CocktailOrderedHandler(_loggerMock, _factoryMock);
        }

        [Fact]
        public async Task Handle_Should_DecreaseStock_When_EnoughIngredientsExist()
        {
            var cocktailId = Guid.NewGuid();
            var bourbonId = Guid.NewGuid();
            var orderQuantity = 2; 

            using (var setupContext = new ApplicationDbContext(_contextOptions))
            {
                var bourbon = new Ingredient { Name = "Bourbon_In_Db", Category = "Alcohol" };
                setupContext.Entry(bourbon).Property(x => x.IngredientId).CurrentValue = bourbonId;
                setupContext.Ingredients.Add(bourbon);

                var cocktail = new Cocktail { Name = "Old Fashioned" };
                setupContext.Entry(cocktail).Property(x => x.CocktailId).CurrentValue = cocktailId;
                cocktail.CocktailIngredients.Add(new CocktailIngredient
                {
                    CocktailId = cocktailId,
                    IngredientId = bourbonId,
                    Amount = 60 
                });
                setupContext.Cocktails.Add(cocktail);

                setupContext.Inventories.Add(new Inventory(bourbonId, 200, "ml"));
                await setupContext.SaveChangesAsync();
            }

            var message = new CocktailOrderedEvent(cocktailId, orderQuantity);

            await _handler.Handle(message, CancellationToken.None);

            using var verificationContext = new ApplicationDbContext(_contextOptions);
            var inventory = await verificationContext.Inventories
                .FirstAsync(x => x.IngredientId == bourbonId);

            Assert.Equal(80, inventory.AvailableAmount);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_And_NotSaveChanges_When_StockIsInsufficient()
        {
            var cocktailId = Guid.NewGuid();
            var ginId = Guid.NewGuid();

            using (var setupContext = new ApplicationDbContext(_contextOptions))
            {
                var gin = new Ingredient { Name = "Gin_In_Db", Category = "Alcohol" };
                setupContext.Entry(gin).Property(x => x.IngredientId).CurrentValue = ginId;
                setupContext.Ingredients.Add(gin);

                var cocktail = new Cocktail { Name = "Gimlet" };
                setupContext.Entry(cocktail).Property(x => x.CocktailId).CurrentValue = cocktailId;
                setupContext.Cocktails.Add(cocktail);
                cocktail.CocktailIngredients.Add(new CocktailIngredient
                {
                    CocktailId = cocktailId,
                    IngredientId = ginId,
                    Amount = 50
                });
                setupContext.Cocktails.Add(cocktail);

                setupContext.Inventories.Add(new Inventory(ginId, 30, "ml"));
                await setupContext.SaveChangesAsync();
            }

            var message = new CocktailOrderedEvent(cocktailId, 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(message, CancellationToken.None));

            using var verificationContext = new ApplicationDbContext(_contextOptions);
            var inventory = await verificationContext.Inventories.FirstAsync(x => x.IngredientId == ginId);
            Assert.Equal(30, inventory.AvailableAmount);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
