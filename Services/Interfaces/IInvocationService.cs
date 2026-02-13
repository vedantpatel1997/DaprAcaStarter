using DaprAcaStarter.Models.Requests;

namespace DaprAcaStarter.Services.Interfaces;

public interface IInvocationService
{
    object Echo(InvocationRequest request);
    Task<object> InvokeSelfAsync(InvocationRequest request, CancellationToken cancellationToken);
}
