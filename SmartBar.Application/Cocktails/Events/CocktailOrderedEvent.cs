using MediatR;

namespace SmartBar.Application.Cocktails.Events
{
    public record CocktailOrderedEvent(Guid CocktailId, int Quantity) : INotification;
}
