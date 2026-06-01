using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SmartBar.Application.Ingredients.Events;
using SmartBar.Domain.Entities;
using SmartBar.Infrastructure;
using SmartBar.Infrastructure.Messaging;

namespace SmartBar.Application.Tests;

public class IngredientCreatedHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
    private readonly IDbContextFactory<ApplicationDbContext> _factoryMock;
    private readonly ILogger<IngredientCreatedHandler> _loggerMock;
    private readonly IngredientCreatedHandler _handler;

    public IngredientCreatedHandlerTests()
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

        _loggerMock = Substitute.For<ILogger<IngredientCreatedHandler>>();
        _handler = new IngredientCreatedHandler(_loggerMock, _factoryMock);
    }

    [Fact]
    public async Task Handle_Should_CreateInventory_When_IngredientDoesNotExistOnStock()
    {
        var bourbonId = Guid.Parse("019e0750-7b56-789a-bcde-f123456789ab");
        var ingredientCreatedEvent = new IngredientCreatedEvent(bourbonId, "Bourbon");
        var cancellationToken = CancellationToken.None;

        using (var setupContext = new ApplicationDbContext(_contextOptions))
        {
            var ingredient = new Ingredient { Name = "Bourbon_In_Db", Category = "Alcohol" };
            setupContext.Entry(ingredient).Property(x => x.IngredientId).CurrentValue = bourbonId;
            setupContext.Ingredients.Add(ingredient);
            await setupContext.SaveChangesAsync(cancellationToken);
        }

        await _handler.Handle(ingredientCreatedEvent, cancellationToken);

        using var verificationContext = new ApplicationDbContext(_contextOptions);
        var addedInventory = await verificationContext.Inventories
            .FirstOrDefaultAsync(x => x.IngredientId == bourbonId, cancellationToken);

        Assert.NotNull(addedInventory);
        Assert.Equal(0, addedInventory.AvailableAmount);
        Assert.Equal("ml", addedInventory.Unit);
        Assert.NotEqual(Guid.Empty, addedInventory.InventoryId);
    }

    [Fact]
    public async Task Handle_Should_NotCreateDuplicate_When_InventoryAlreadyExists()
    {
        var bourbonId = Guid.Parse("019e0750-7b56-789a-bcde-f123456789ab");
        var ingredientCreatedEvent = new IngredientCreatedEvent(bourbonId, "Bourbon");
        var cancellationToken = CancellationToken.None;

        using (var setupContext = new ApplicationDbContext(_contextOptions))
        {
            var ingredient = new Ingredient { Name = "Bourbon_In_Db", Category = "Alcohol" };
            setupContext.Entry(ingredient).Property(x => x.IngredientId).CurrentValue = bourbonId;
            setupContext.Ingredients.Add(ingredient);

            setupContext.Inventories.Add(new Inventory(bourbonId, 500, "ml"));
            await setupContext.SaveChangesAsync(cancellationToken);
        }

        await _handler.Handle(ingredientCreatedEvent, cancellationToken);

        using var verificationContext = new ApplicationDbContext(_contextOptions);
        var count = await verificationContext.Inventories
            .CountAsync(x => x.IngredientId == bourbonId, cancellationToken);

        Assert.Equal(1, count);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}