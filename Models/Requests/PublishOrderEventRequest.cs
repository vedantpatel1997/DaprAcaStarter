using System.ComponentModel.DataAnnotations;

namespace DaprAcaStarter.Models.Requests;

public sealed record PublishOrderEventRequest([Required] string OrderId, [Required] string Status);
