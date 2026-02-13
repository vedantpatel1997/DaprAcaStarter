using DaprAcaStarter.Models.Responses;

namespace DaprAcaStarter.Services.Interfaces;

public interface IAppInfoService
{
    ServiceInfoResponse GetServiceInfo();
    HealthResponse GetHealth();
}
