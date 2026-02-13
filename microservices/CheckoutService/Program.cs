using Dapr.Client;
using System.Collections.Concurrent;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

var useDapr = builder.Configuration.GetValue("UseDapr", false);
var cartServiceBaseUrl = builder.Configuration.GetValue("CartServiceBaseUrl", "http://localhost:8082")!;

builder.Services.AddDaprClient();
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

const string stateStoreName = "statestore";
const string checkoutTopic = "checkout.completed.v1";
var localOrders = new ConcurrentDictionary<string, CheckoutOrder>(StringComparer.OrdinalIgnoreCase);

app.MapGet("/", () => Results.Ok(new
{
    appId = "checkout-service",
    mode = useDapr ? "dapr" : "direct",
    message = "Checkout microservice is running",
    stateStore = useDapr ? stateStoreName : "in-memory",
    topic = useDapr ? checkoutTopic : "none"
}));

app.MapPost("/checkout/{customerId}", async (string customerId, DaprClient daprClient, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    CartState cart;
    if (useDapr)
    {
        cart = await daprClient.InvokeMethodAsync<CartState>(
            HttpMethod.Get,
            "cart-service",
            $"cart/{Uri.EscapeDataString(customerId)}",
            cancellationToken: cancellationToken);
    }
    else
    {
        var client = httpClientFactory.CreateClient();
        var cartResponse = await client.GetAsync($"{cartServiceBaseUrl}/cart/{Uri.EscapeDataString(customerId)}", cancellationToken);
        cartResponse.EnsureSuccessStatusCode();
        cart = (await cartResponse.Content.ReadFromJsonAsync<CartState>(cancellationToken))!;
    }

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

    if (useDapr)
    {
        await daprClient.SaveStateAsync(stateStoreName, BuildOrderKey(order.OrderId), order, cancellationToken: cancellationToken);

        var checkoutEvent = new CheckoutCompletedEvent
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Total = order.Total,
            CheckedOutUtc = order.CheckedOutUtc
        };

        await daprClient.PublishEventAsync("pubsub", checkoutTopic, checkoutEvent, cancellationToken);
    }
    else
    {
        localOrders[BuildOrderKey(order.OrderId)] = order;
        var client = httpClientFactory.CreateClient();
        await client.DeleteAsync($"{cartServiceBaseUrl}/cart/{Uri.EscapeDataString(customerId)}", cancellationToken);
    }

    return Results.Ok(order);
});

app.MapGet("/orders/{orderId}", async (string orderId, DaprClient daprClient, CancellationToken cancellationToken) =>
{
    if (useDapr)
    {
        var order = await daprClient.GetStateAsync<CheckoutOrder?>(stateStoreName, BuildOrderKey(orderId), cancellationToken: cancellationToken);
        return order is null
            ? Results.NotFound(new { message = "Order not found", orderId })
            : Results.Ok(order);
    }

    return localOrders.TryGetValue(BuildOrderKey(orderId), out var localOrder)
        ? Results.Ok(localOrder)
        : Results.NotFound(new { message = "Order not found", orderId });
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
