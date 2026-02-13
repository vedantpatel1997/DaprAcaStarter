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
            "Dapr ACA Starter is running",
            _daprOptions.AppId,
            new DaprServiceInfo(_daprOptions.StateStoreName, _daprOptions.PubSubName, _daprOptions.OrdersTopic));
    }

    public HealthResponse GetHealth()
    {
        return new HealthResponse("ok", DateTime.UtcNow);
    }
}
