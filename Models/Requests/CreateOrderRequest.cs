using System.ComponentModel.DataAnnotations;

namespace DaprAcaStarter.Models.Requests;

public sealed record CreateOrderRequest(
    [Required] string CustomerId,
    [Required] string Product,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(0.01, double.MaxValue)] decimal UnitPrice);
