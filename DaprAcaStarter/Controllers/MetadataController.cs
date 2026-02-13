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
            "Service started. appId={AppId}, products={ProductsAppId}, cart={CartAppId}, checkout={CheckoutAppId}, topic={Topic}",
            serviceInfo.AppId,
            serviceInfo.Services.ProductsAppId,
            serviceInfo.Services.CartAppId,
            serviceInfo.Services.CheckoutAppId,
            serviceInfo.Dapr.Topic);

        return Ok(serviceInfo);
    }

    [HttpGet("healthz")]
    public IActionResult GetHealth()
    {
        return Ok(_appInfoService.GetHealth());
    }
}
