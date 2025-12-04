using DotNetEnv.Configuration;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using SmartBin.Api.GenericRepository;
using SmartBin.Api.GenericRepository;
using SmartBin.Api.Models;
using SmartBin.Api.Mqtt;
using SmartBin.Api.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.Configuration
    .AddEnvironmentVariables()
    .AddDotNetEnv();

builder.Host.UseSerilog((context, configuration) =>
                            configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddHostedService<MqttClientService>();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("Database"));

builder.Services.AddSingleton<IMongoSettings>(provider =>
    provider.GetRequiredService<IOptions<MongoSettings>>().Value);
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBinService, BinService>();


var app = builder.Build();
app.UseSerilogRequestLogging();

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
