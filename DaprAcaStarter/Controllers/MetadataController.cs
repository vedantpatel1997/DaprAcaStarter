using DaprAcaStarter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DaprAcaStarter.Controllers;

[ApiController]
[Route("")]
public sealed class MetadataController(IAppInfoService appInfoService, ILogger<MetadataController> logger) : ControllerBase
{
    private readonly IAppInfoService _appInfoService = appInfoService;
    private readonly ILogger<MetadataController> _logger = logger;

    [HttpGet]
    public IActionResult GetServiceInfo()
    {
        var serviceInfo = _appInfoService.GetServiceInfo();
        _logger.LogInformation(
            "Service started. appId={AppId}, stateStore={StateStoreName}, pubsub={PubSubName}, topic={OrdersTopic}",
            serviceInfo.AppId,
            serviceInfo.Dapr.StateStore,
            serviceInfo.Dapr.Pubsub,
            serviceInfo.Dapr.Topic);

        return Ok(serviceInfo);
    }

    [HttpGet("healthz")]
    public IActionResult GetHealth()
    {
        return Ok(_appInfoService.GetHealth());
    }
}
