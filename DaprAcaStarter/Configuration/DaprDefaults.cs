namespace DaprAcaStarter.Configuration;

public static class DaprDefaults
{
    public const string AppId = "storefront-api";
    public const bool UseDaprInvocation = true;
    public const string StateStoreName = "statestore";
    public const string PubSubName = "pubsub";
    public const string OrdersTopic = "checkout.completed.v1";
    public const string ProductsAppId = "products-service";
    public const string CartAppId = "cart-service";
    public const string CheckoutAppId = "checkout-service";
    public const string ProductsBaseUrl = "http://localhost:8081";
    public const string CartBaseUrl = "http://localhost:8082";
    public const string CheckoutBaseUrl = "http://localhost:8083";
    public const string FrontendCorsPolicy = "frontend";
}
