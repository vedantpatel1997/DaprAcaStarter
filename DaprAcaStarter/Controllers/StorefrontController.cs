using DaprAcaStarter.Models.Requests;
using DaprAcaStarter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DaprAcaStarter.Controllers;

[ApiController]
[Route("api")]
public sealed class StorefrontController(IStorefrontService storefrontService) : ControllerBase
{
    private readonly IStorefrontService _storefrontService = storefrontService;

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _storefrontService.GetProductsAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("cart/{customerId}")]
    public async Task<IActionResult> GetCart(string customerId, CancellationToken cancellationToken)
    {
        var cart = await _storefrontService.GetCartAsync(customerId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("cart/{customerId}/items")]
    public async Task<IActionResult> AddCartItem(
        string customerId,
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await _storefrontService.AddCartItemAsync(customerId, request, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("checkout/{customerId}")]
    public async Task<IActionResult> Checkout(string customerId, CancellationToken cancellationToken)
    {
        var order = await _storefrontService.CheckoutAsync(customerId, cancellationToken);
        return Ok(order);
    }

    [HttpGet("orders/{orderId}")]
    public async Task<IActionResult> GetOrder(string orderId, CancellationToken cancellationToken)
    {
        var order = await _storefrontService.GetOrderAsync(orderId, cancellationToken);
        return order is null
            ? NotFound(new { message = "Order not found", orderId })
            : Ok(order);
    }
}
