namespace DaprAcaStarter.Models.Store;

public sealed record Product(string Id, string Name, decimal Price, string Currency, string Description);

public sealed record CartItem(string ProductId, string ProductName, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed record Cart(string CustomerId, IReadOnlyList<CartItem> Items)
{
    public decimal Total => Items.Sum(item => item.LineTotal);
}

public sealed record CheckoutOrder(
    string OrderId,
    string CustomerId,
    IReadOnlyList<CartItem> Items,
    decimal Total,
    DateTime CheckedOutUtc,
    string Status);
