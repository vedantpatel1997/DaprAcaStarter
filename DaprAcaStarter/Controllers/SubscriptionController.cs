using Dapr;
using DaprAcaStarter.Configuration;
using DaprAcaStarter.Models;
using DaprAcaStarter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DaprAcaStarter.Controllers;

[ApiController]
[Route("dapr")]
public sealed class SubscriptionController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    [HttpPost("orders-subscription")]
    [Topic(DaprDefaults.PubSubName, DaprDefaults.OrdersTopic)]
    public async Task<IActionResult> HandleOrdersSubscription([FromBody] Order order, CancellationToken cancellationToken)
    {
        await _orderService.HandleSubscriptionAsync(order, cancellationToken);
        return Ok();
    }
}
