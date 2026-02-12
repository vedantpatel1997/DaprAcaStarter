using System.ComponentModel.DataAnnotations;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

const string AppId = "dapr-aca-starter";
const string StateStoreName = "statestore";
const string PubSubName = "pubsub";
const string OrdersTopic = "orders.v1";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff zzz ";
});

builder.Services.AddOpenApi();
builder.Services.AddDaprClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCloudEvents();
app.MapSubscribeHandler();

app.MapGet("/", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("Startup");
    logger.LogInformation("Service started. appId={AppId}, stateStore={StateStoreName}, pubsub={PubSubName}, topic={OrdersTopic}", AppId, StateStoreName, PubSubName, OrdersTopic);

    return Results.Ok(new
    {
        message = "Dapr ACA Starter is running",
        appId = AppId,
        dapr = new
        {
            stateStore = StateStoreName,
            pubsub = PubSubName,
            topic = OrdersTopic
        }
    });
});

app.MapGet("/healthz", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));

app.MapPost("/orders", async ([FromBody] CreateOrderRequest request, DaprClient daprClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("Orders");

    var order = new Order
    {
        Id = Guid.NewGuid().ToString("N"),
        CustomerId = request.CustomerId,
        Product = request.Product,
        Quantity = request.Quantity,
        UnitPrice = request.UnitPrice,
        CreatedUtc = DateTime.UtcNow
    };

    await daprClient.SaveStateAsync(StateStoreName, order.Id, order, cancellationToken: cancellationToken);
    logger.LogInformation("Order saved to state store. orderId={OrderId}, customerId={CustomerId}, total={Total}", order.Id, order.CustomerId, order.Total);

    await daprClient.PublishEventAsync(PubSubName, OrdersTopic, order, cancellationToken);
    logger.LogInformation("Order published to topic. orderId={OrderId}, topic={Topic}", order.Id, OrdersTopic);

    return Results.Created($"/orders/{order.Id}", order);
});

app.MapGet("/orders/{id}", async (string id, DaprClient daprClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("Orders");

    var order = await daprClient.GetStateAsync<Order?>(StateStoreName, id, cancellationToken: cancellationToken);
    if (order is null)
    {
        logger.LogWarning("Order not found. orderId={OrderId}", id);
        return Results.NotFound(new { message = "Order not found", orderId = id });
    }

    logger.LogInformation("Order read from state store. orderId={OrderId}", id);
    return Results.Ok(order);
});

app.MapPost("/publish/orders", async ([FromBody] PublishOrderEventRequest request, DaprClient daprClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("PubSub");

    var evt = new
    {
        request.OrderId,
        request.Status,
        atUtc = DateTime.UtcNow
    };

    await daprClient.PublishEventAsync(PubSubName, OrdersTopic, evt, cancellationToken);
    logger.LogInformation("Manual publish sent. orderId={OrderId}, status={Status}", request.OrderId, request.Status);

    return Results.Accepted();
});

app.MapPost("/internal/echo", ([FromBody] InvocationRequest request, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("Invocation");
    logger.LogInformation("Internal endpoint invoked via Dapr service invocation. message={Message}", request.Message);

    return Results.Ok(new
    {
        echoed = request.Message,
        receivedUtc = DateTime.UtcNow,
        by = AppId
    });
});

app.MapPost("/invoke/self", async ([FromBody] InvocationRequest request, DaprClient daprClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("Invocation");

    var response = await daprClient.InvokeMethodAsync<InvocationRequest, object>(
        AppId,
        "internal/echo",
        request,
        cancellationToken);

    logger.LogInformation("Service invocation call completed. targetAppId={TargetAppId}", AppId);
    return Results.Ok(response);
});

app.MapPost("/dapr/orders-subscription", [Topic(PubSubName, OrdersTopic)] ([FromBody] Order order, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("Subscriber");
    logger.LogInformation(
        "Subscriber received order event. orderId={OrderId}, customerId={CustomerId}, product={Product}, qty={Quantity}, total={Total}",
        order.Id,
        order.CustomerId,
        order.Product,
        order.Quantity,
        order.Total);

    return Results.Ok();
});

app.Run();

public sealed record CreateOrderRequest(
    [Required] string CustomerId,
    [Required] string Product,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(0.01, double.MaxValue)] decimal UnitPrice);

public sealed record PublishOrderEventRequest([Required] string OrderId, [Required] string Status);

public sealed record InvocationRequest([Required] string Message);

public sealed class Order
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public string Product { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    public DateTime CreatedUtc { get; set; }

    public decimal Total => Quantity * UnitPrice;
}
