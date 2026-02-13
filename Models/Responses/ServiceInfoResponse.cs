namespace DaprAcaStarter.Models.Responses;

public sealed record ServiceInfoResponse(string Message, string AppId, DaprServiceInfo Dapr);

public sealed record DaprServiceInfo(string StateStore, string Pubsub, string Topic);
