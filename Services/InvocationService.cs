using Dapr.Client;
using DaprAcaStarter.Models.Requests;
using DaprAcaStarter.Services.Interfaces;
using DaprAcaStarter.Configuration;
using Microsoft.Extensions.Options;

namespace DaprAcaStarter.Services;

public sealed class InvocationService(
    DaprClient daprClient,
    IOptions<DaprOptions> options,
    ILogger<InvocationService> logger) : IInvocationService
{
    private readonly DaprClient _daprClient = daprClient;
    private readonly DaprOptions _daprOptions = options.Value;
    private readonly ILogger<InvocationService> _logger = logger;

    public object Echo(InvocationRequest request)
    {
        _logger.LogInformation("Internal endpoint invoked via Dapr service invocation. message={Message}", request.Message);

        return new
        {
            Echoed = request.Message,
            ReceivedUtc = DateTime.UtcNow,
            By = _daprOptions.AppId
        };
    }

    public async Task<object> InvokeSelfAsync(InvocationRequest request, CancellationToken cancellationToken)
    {
        var response = await _daprClient.InvokeMethodAsync<InvocationRequest, object>(
            _daprOptions.AppId,
            "internal/echo",
            request,
            cancellationToken);

        _logger.LogInformation("Service invocation call completed. targetAppId={TargetAppId}", _daprOptions.AppId);

        return response;
    }
}
