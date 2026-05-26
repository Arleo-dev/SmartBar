using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using SmartBar.Application.Ingredients.Commands;
using SmartBar.Application.Ingredients.Events;
using SmartBar.Application.Interfaces;
using SmartBar.Domain.Entities;
using Wolverine;

namespace SmartBar.Application.Tests;

public class CreateIngredientCommandHandlerTests
{
    private readonly IApplicationDbContext _contextMock;
    private readonly IDistributedCache _cacheMock;
    private readonly IMessageBus _busMock;
    private readonly CreateIngredientCommandHandler _handler;

    public CreateIngredientCommandHandlerTests()
    {
        _contextMock = Substitute.For<IApplicationDbContext>();
        _cacheMock = Substitute.For<IDistributedCache>();
        _busMock = Substitute.For<IMessageBus>();
        _contextMock.Ingredients.Returns(Substitute.For<DbSet<Ingredient>>());
        _handler = new CreateIngredientCommandHandler(_contextMock, _cacheMock, _busMock);
    }

    [Fact]
    public async Task Handle_Should_CreateIngredient_And_PublishEvent_WhenCommandIsValid()
    {
        var command = new CreateIngredientCommand("Bourbon", "Alcohol");
        var cancellationToken = CancellationToken.None;

        Guid resultId = await _handler.Handle(command, cancellationToken);

        Assert.NotEqual(Guid.Empty, resultId);


        _contextMock.Ingredients.Received(1).Add(Arg.Is<Ingredient>(i => i.Name == command.Name));

        await _contextMock.Received(1).SaveChangesAsync(cancellationToken);

        await _cacheMock.Received(1).RemoveAsync("ingredients_list", cancellationToken);

        await _busMock.Received(1).PublishAsync(
            Arg.Is<IngredientCreatedEvent>(e => e.Name == command.Name && e.IngredientId == resultId));
    }
}