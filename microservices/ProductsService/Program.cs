var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var products = new List<Product>
{
    new("P-100", "Mechanical Keyboard", 89.00m, "USD", "75% layout with hot-swappable switches"),
    new("P-200", "Ergonomic Mouse", 54.00m, "USD", "Vertical mouse for reduced wrist strain"),
    new("P-300", "4K Monitor", 329.00m, "USD", "27-inch IPS monitor with USB-C"),
    new("P-400", "Laptop Stand", 39.99m, "USD", "Aluminum stand for better posture")
};

app.MapGet("/", () => Results.Ok(new
{
    appId = "products-service",
    message = "Products microservice is running",
    count = products.Count
}));

app.MapGet("/products", () => Results.Ok(products));

app.MapGet("/products/{id}", (string id) =>
{
    var product = products.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    return product is null ? Results.NotFound(new { message = "Product not found", productId = id }) : Results.Ok(product);
});

app.Run();

public sealed record Product(string Id, string Name, decimal Price, string Currency, string Description);
