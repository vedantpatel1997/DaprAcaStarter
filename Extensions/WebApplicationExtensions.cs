using DaprAcaStarter.Configuration;

namespace DaprAcaStarter.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseCloudEvents();
        app.UseCors(DaprDefaults.FrontendCorsPolicy);

        app.MapSubscribeHandler();
        app.MapControllers();

        return app;
    }
}
