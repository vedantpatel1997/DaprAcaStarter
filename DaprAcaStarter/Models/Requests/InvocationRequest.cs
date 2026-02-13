using System.ComponentModel.DataAnnotations;

namespace DaprAcaStarter.Models.Requests;

public sealed record InvocationRequest([Required] string Message);
