using Dapr.Client;
using DaprAcaStarter.Configuration;
using DaprAcaStarter.Models;
using DaprAcaStarter.Models.Requests;
using DaprAcaStarter.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DaprAcaStarter.Services;

public sealed class OrderService(
    DaprClient daprClient,
    IOptions<DaprOptions> options,
    ILogger<OrderService> logger) : IOrderService
{
    private readonly DaprClient _daprClient = daprClient;
    private readonly DaprOptions _daprOptions = options.Value;
    private readonly ILogger<OrderService> _logger = logger;

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid().ToString("N"),
            CustomerId = request.CustomerId,
            Product = request.Product,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            CreatedUtc = DateTime.UtcNow
        };

        await _daprClient.SaveStateAsync(_daprOptions.StateStoreName, order.Id, order, cancellationToken: cancellationToken);
        _logger.LogInformation(
            "Order saved to state store. orderId={OrderId}, customerId={CustomerId}, total={Total}",
            order.Id,
            order.CustomerId,
            order.Total);

        await _daprClient.PublishEventAsync(_daprOptions.PubSubName, _daprOptions.OrdersTopic, order, cancellationToken);
        _logger.LogInformation("Order published to topic. orderId={OrderId}, topic={Topic}", order.Id, _daprOptions.OrdersTopic);

        return order;
    }

    public async Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken)
    {
        var order = await _daprClient.GetStateAsync<Order?>(_daprOptions.StateStoreName, id, cancellationToken: cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Order not found. orderId={OrderId}", id);
            return null;
        }

        _logger.LogInformation("Order read from state store. orderId={OrderId}", id);
        return order;
    }

    public async Task PublishOrderEventAsync(PublishOrderEventRequest request, CancellationToken cancellationToken)
    {
        var evt = new
        {
            request.OrderId,
            request.Status,
            AtUtc = DateTime.UtcNow
        };

        await _daprClient.PublishEventAsync(_daprOptions.PubSubName, _daprOptions.OrdersTopic, evt, cancellationToken);
        _logger.LogInformation("Manual publish sent. orderId={OrderId}, status={Status}", request.OrderId, request.Status);
    }

    public Task HandleSubscriptionAsync(Order order, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Subscriber received order event. orderId={OrderId}, customerId={CustomerId}, product={Product}, qty={Quantity}, total={Total}",
            order.Id,
            order.CustomerId,
            order.Product,
            order.Quantity,
            order.Total);

        return Task.CompletedTask;
    }
}
