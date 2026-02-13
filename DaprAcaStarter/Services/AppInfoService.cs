using DaprAcaStarter.Configuration;
using DaprAcaStarter.Models.Responses;
using DaprAcaStarter.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DaprAcaStarter.Services;

public sealed class AppInfoService(IOptions<DaprOptions> options) : IAppInfoService
{
    private readonly DaprOptions _daprOptions = options.Value;

    public ServiceInfoResponse GetServiceInfo()
    {
        return new ServiceInfoResponse(
            "Storefront API is running",
            _daprOptions.AppId,
            new DaprServiceInfo(_daprOptions.StateStoreName, _daprOptions.PubSubName, _daprOptions.OrdersTopic),
            new DownstreamServices(_daprOptions.ProductsAppId, _daprOptions.CartAppId, _daprOptions.CheckoutAppId),
            [
                "Frontend calls Storefront API",
                "Storefront invokes products/cart/checkout by Dapr app-id",
                "Cart state is stored in statestore",
                "Checkout publishes checkout.completed.v1 to pubsub",
                "Cart service subscribes and clears cart"
            ]);
    }

    public HealthResponse GetHealth()
    {
        return new HealthResponse("ok", DateTime.UtcNow);
    }
}
