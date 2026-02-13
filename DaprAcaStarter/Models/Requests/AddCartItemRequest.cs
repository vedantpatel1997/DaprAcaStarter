using System.ComponentModel.DataAnnotations;

namespace DaprAcaStarter.Models.Requests;

public sealed class AddCartItemRequest
{
    [Required]
    public string ProductId { get; set; } = string.Empty;

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal UnitPrice { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }
}
