using DaprAcaStarter.Configuration;
using DaprAcaStarter.Services;
using DaprAcaStarter.Services.Interfaces;

namespace DaprAcaStarter.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddControllers().AddDapr();
        services.AddDaprClient();

        services.AddOptions<DaprOptions>()
            .Bind(configuration.GetSection(DaprOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddCors(options =>
        {
            options.AddPolicy(DaprDefaults.FrontendCorsPolicy, policy =>
            {
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddScoped<IAppInfoService, AppInfoService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInvocationService, InvocationService>();

        return services;
    }
}
