using DaprAcaStarter.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.ConfigurePipeline();

app.Run();
