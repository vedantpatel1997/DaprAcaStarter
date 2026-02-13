namespace DaprAcaStarter.Models.Responses;

public sealed record ServiceInfoResponse(
    string Message,
    string AppId,
    DaprServiceInfo Dapr,
    DownstreamServices Services,
    string[] Workflow);

public sealed record DaprServiceInfo(string StateStore, string Pubsub, string Topic);

public sealed record DownstreamServices(string ProductsAppId, string CartAppId, string CheckoutAppId);
