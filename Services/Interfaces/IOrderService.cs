using DaprAcaStarter.Models;
using DaprAcaStarter.Models.Requests;

namespace DaprAcaStarter.Services.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken);
    Task PublishOrderEventAsync(PublishOrderEventRequest request, CancellationToken cancellationToken);
    Task HandleSubscriptionAsync(Order order, CancellationToken cancellationToken);
}
