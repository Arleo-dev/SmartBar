using MediatR;
using SmartBar.Application.Cocktails.Events;
using Wolverine;

namespace SmartBar.Application.Cocktails.Commands
{
    public record OrderCocktailCommand(Guid CocktailId, int Quantity) : IRequest;

    public class OrderCocktailCommandHandler : IRequestHandler<OrderCocktailCommand>
    {
        private readonly IMessageBus _bus;

        public OrderCocktailCommandHandler(IMessageBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(OrderCocktailCommand request, CancellationToken cancellationToken)
        {
            await _bus.PublishAsync(new CocktailOrderedEvent(request.CocktailId, request.Quantity));
        }
    }
}
