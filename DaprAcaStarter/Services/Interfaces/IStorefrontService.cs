using DaprAcaStarter.Models.Requests;
using DaprAcaStarter.Models.Store;

namespace DaprAcaStarter.Services.Interfaces;

public interface IStorefrontService
{
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken);
    Task<Cart> GetCartAsync(string customerId, CancellationToken cancellationToken);
    Task<Cart> AddCartItemAsync(string customerId, AddCartItemRequest request, CancellationToken cancellationToken);
    Task<CheckoutOrder> CheckoutAsync(string customerId, CancellationToken cancellationToken);
    Task<CheckoutOrder?> GetOrderAsync(string orderId, CancellationToken cancellationToken);
}
