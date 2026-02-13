using DaprAcaStarter.Models.Requests;
using DaprAcaStarter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DaprAcaStarter.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderService.CreateOrderAsync(request, cancellationToken);
        return Created($"/orders/{order.Id}", order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Order not found", orderId = id });
        }

        return Ok(order);
    }

    [HttpPost("~/publish/orders")]
    public async Task<IActionResult> PublishOrderEvent([FromBody] PublishOrderEventRequest request, CancellationToken cancellationToken)
    {
        await _orderService.PublishOrderEventAsync(request, cancellationToken);
        return Accepted();
    }
}
