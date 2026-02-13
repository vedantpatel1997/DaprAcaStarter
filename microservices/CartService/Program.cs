using Dapr;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCloudEvents();
app.MapSubscribeHandler();

const string stateStoreName = "statestore";

app.MapGet("/", () => Results.Ok(new
{
    appId = "cart-service",
    message = "Cart microservice is running",
    stateStore = stateStoreName,
    subscriptions = new[] { "checkout.completed.v1" }
}));

app.MapGet("/cart/{customerId}", async (string customerId, DaprClient daprClient, CancellationToken cancellationToken) =>
{
    var key = BuildCartKey(customerId);
    var cart = await daprClient.GetStateAsync<CartState?>(stateStoreName, key, cancellationToken: cancellationToken);
    return Results.Ok(cart ?? new CartState(customerId, []));
});

app.MapPost("/cart/{customerId}/items", async (string customerId, AddCartItemRequest request, DaprClient daprClient, CancellationToken cancellationToken) =>
{
    var key = BuildCartKey(customerId);
    var cart = await daprClient.GetStateAsync<CartState?>(stateStoreName, key, cancellationToken: cancellationToken)
        ?? new CartState(customerId, []);

    var existing = cart.Items.FirstOrDefault(item => item.ProductId.Equals(request.ProductId, StringComparison.OrdinalIgnoreCase));
    if (existing is null)
    {
        cart.Items.Add(new CartItem(request.ProductId, request.ProductName, request.UnitPrice, request.Quantity));
    }
    else
    {
        existing.Quantity += request.Quantity;
    }

    await daprClient.SaveStateAsync(stateStoreName, key, cart, cancellationToken: cancellationToken);
    return Results.Ok(cart);
});

app.MapDelete("/cart/{customerId}", async (string customerId, DaprClient daprClient, CancellationToken cancellationToken) =>
{
    await daprClient.DeleteStateAsync(stateStoreName, BuildCartKey(customerId), cancellationToken: cancellationToken);
    return Results.NoContent();
});

app.MapPost("/checkout-events", async (CheckoutCompletedEvent evt, DaprClient daprClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("CheckoutEventHandler");
    await daprClient.DeleteStateAsync(stateStoreName, BuildCartKey(evt.CustomerId), cancellationToken: cancellationToken);
    logger.LogInformation("Cleared cart for customer {CustomerId} after checkout order {OrderId}", evt.CustomerId, evt.OrderId);
    return Results.Ok();
})
.WithTopic("pubsub", "checkout.completed.v1");

app.Run();

static string BuildCartKey(string customerId) => $"cart:{customerId}";

public sealed class AddCartItemRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public sealed class CartState
{
    public CartState(string customerId, List<CartItem> items)
    {
        CustomerId = customerId;
        Items = items;
    }

    public string CustomerId { get; }
    public List<CartItem> Items { get; }
    public decimal Total => Items.Sum(item => item.LineTotal);
}

public sealed class CartItem
{
    public CartItem(string productId, string productName, decimal unitPrice, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public string ProductId { get; }
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed class CheckoutCompletedEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CheckedOutUtc { get; set; }
}
