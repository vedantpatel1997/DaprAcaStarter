using System.ComponentModel.DataAnnotations;

namespace DaprAcaStarter.Configuration;

public sealed class DaprOptions
{
    public const string SectionName = "Dapr";

    [Required]
    public string AppId { get; set; } = DaprDefaults.AppId;

    [Required]
    public string StateStoreName { get; set; } = DaprDefaults.StateStoreName;

    [Required]
    public string PubSubName { get; set; } = DaprDefaults.PubSubName;

    [Required]
    public string OrdersTopic { get; set; } = DaprDefaults.OrdersTopic;

    [Required]
    public string ProductsAppId { get; set; } = DaprDefaults.ProductsAppId;

    [Required]
    public string CartAppId { get; set; } = DaprDefaults.CartAppId;

    [Required]
    public string CheckoutAppId { get; set; } = DaprDefaults.CheckoutAppId;
}
