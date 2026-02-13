using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

const string stateStoreName = "statestore";
const string checkoutTopic = "checkout.completed.v1";

app.MapGet("/", () => Results.Ok(new
{
    appId = "checkout-service",
    message = "Checkout microservice is running",
    stateStore = stateStoreName,
    topic = checkoutTopic
}));

app.MapPost("/checkout/{customerId}", async (string customerId, DaprClient daprClient, CancellationToken cancellationToken) =>
{
    var cart = await daprClient.InvokeMethodAsync<CartState>(
        HttpMethod.Get,
        "cart-service",
        $"cart/{Uri.EscapeDataString(customerId)}",
        cancellationToken: cancellationToken);

    if (cart.Items.Count == 0)
    {
        return Results.BadRequest(new { message = "Cart is empty", customerId });
    }

    var order = new CheckoutOrder
    {
        OrderId = Guid.NewGuid().ToString("N"),
        CustomerId = customerId,
        Items = cart.Items,
        Total = cart.Total,
        CheckedOutUtc = DateTime.UtcNow,
        Status = "Confirmed"
    };

    await daprClient.SaveStateAsync(stateStoreName, BuildOrderKey(order.OrderId), order, cancellationToken: cancellationToken);

    var checkoutEvent = new CheckoutCompletedEvent
    {
        OrderId = order.OrderId,
        CustomerId = order.CustomerId,
        Total = order.Total,
        CheckedOutUtc = order.CheckedOutUtc
    };

    await daprClient.PublishEventAsync("pubsub", checkoutTopic, checkoutEvent, cancellationToken);

    return Results.Ok(order);
});

app.MapGet("/orders/{orderId}", async (string orderId, DaprClient daprClient, CancellationToken cancellationToken) =>
{
    var order = await daprClient.GetStateAsync<CheckoutOrder?>(stateStoreName, BuildOrderKey(orderId), cancellationToken: cancellationToken);
    return order is null
        ? Results.NotFound(new { message = "Order not found", orderId })
        : Results.Ok(order);
});

app.Run();

static string BuildOrderKey(string orderId) => $"order:{orderId}";

public sealed class CartState
{
    public string CustomerId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = [];
    public decimal Total { get; set; }
}

public sealed class CartItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class CheckoutOrder
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = [];
    public decimal Total { get; set; }
    public DateTime CheckedOutUtc { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class CheckoutCompletedEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CheckedOutUtc { get; set; }
}
