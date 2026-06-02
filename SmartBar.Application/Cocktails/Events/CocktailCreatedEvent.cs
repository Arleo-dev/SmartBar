using MediatR;

namespace SmartBar.Application.Cocktails.Events
{
    public record CocktailCreatedEvent(Guid CocktailId, string Name) : INotification;
}
