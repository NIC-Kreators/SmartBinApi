using DotNetEnv.Configuration;
using Scalar.AspNetCore;
using SmartBin.Api.Mqtt;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.Configuration
    .AddEnvironmentVariables()
    .AddDotNetEnv();

builder.Services.AddHostedService<MqttClientService>();

var app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options
            .WithTitle("SmartBin API")
            .WithTheme(ScalarTheme.DeepSpace)
            .ShowOperationId()
            .WithDefaultHttpClient(ScalarTarget.Node, ScalarClient.Undici)
            .AddPreferredSecuritySchemes("BearerAuth")
            .AddHttpAuthentication("BearerAuth", auth =>
            {
                auth.Token = "Your jwt token";
            });
    });
}

app.MapGet("/hello", () => "Hello").Stable();

app.Run();
