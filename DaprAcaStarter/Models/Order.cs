using System.ComponentModel.DataAnnotations;

namespace DaprAcaStarter.Models;

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
