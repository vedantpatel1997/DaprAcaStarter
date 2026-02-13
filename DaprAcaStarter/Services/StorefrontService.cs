using Dapr.Client;
using DaprAcaStarter.Configuration;
using DaprAcaStarter.Models.Requests;
using DaprAcaStarter.Models.Store;
using DaprAcaStarter.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DaprAcaStarter.Services;

public sealed class StorefrontService(
    DaprClient daprClient,
    IOptions<DaprOptions> options,
    ILogger<StorefrontService> logger) : IStorefrontService
{
    private readonly DaprClient _daprClient = daprClient;
    private readonly DaprOptions _daprOptions = options.Value;
    private readonly ILogger<StorefrontService> _logger = logger;

    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken)
    {
        var products = await _daprClient.InvokeMethodAsync<List<Product>>(
            HttpMethod.Get,
            _daprOptions.ProductsAppId,
            "products",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Fetched {Count} products from {AppId}", products.Count, _daprOptions.ProductsAppId);
        return products;
    }

    public async Task<Cart> GetCartAsync(string customerId, CancellationToken cancellationToken)
    {
        var cart = await _daprClient.InvokeMethodAsync<Cart>(
            HttpMethod.Get,
            _daprOptions.CartAppId,
            $"cart/{Uri.EscapeDataString(customerId)}",
            cancellationToken: cancellationToken);

        return cart;
    }

    public async Task<Cart> AddCartItemAsync(string customerId, AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var cart = await _daprClient.InvokeMethodAsync<AddCartItemRequest, Cart>(
            HttpMethod.Post,
            _daprOptions.CartAppId,
            $"cart/{Uri.EscapeDataString(customerId)}/items",
            request,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Added product {ProductId} to cart for {CustomerId}", request.ProductId, customerId);
        return cart;
    }

    public async Task<CheckoutOrder> CheckoutAsync(string customerId, CancellationToken cancellationToken)
    {
        var result = await _daprClient.InvokeMethodAsync<object?, CheckoutOrder>(
            HttpMethod.Post,
            _daprOptions.CheckoutAppId,
            $"checkout/{Uri.EscapeDataString(customerId)}",
            null,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Checkout completed for {CustomerId} with order {OrderId}", customerId, result.OrderId);
        return result;
    }

    public async Task<CheckoutOrder?> GetOrderAsync(string orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _daprClient.InvokeMethodAsync<CheckoutOrder>(
                HttpMethod.Get,
                _daprOptions.CheckoutAppId,
                $"orders/{Uri.EscapeDataString(orderId)}",
                cancellationToken: cancellationToken);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load order {OrderId} from {AppId}", orderId, _daprOptions.CheckoutAppId);
            return null;
        }
    }
}
