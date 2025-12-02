using DotNetEnv.Configuration;
using Scalar.AspNetCore;
using Serilog;
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
builder.Services.AddSingleton<MongoDbService>(); // сервис с коллекциями

builder.Services.AddScoped<IRepository<User>>(sp =>
    new MongoRepository<User>(sp.GetRequiredService<MongoDbService>().Users));

builder.Services.AddScoped<IRepository<Bin>>(sp =>
    new MongoRepository<Bin>(sp.GetRequiredService<MongoDbService>().Bins));

builder.Services.AddScoped<IRepository<CleaningUp>>(sp =>
    new MongoRepository<CleaningUp>(sp.GetRequiredService<MongoDbService>().CleaningUps));

builder.Services.AddScoped<IRepository<ShiftLog>>(sp =>
    new MongoRepository<ShiftLog>(sp.GetRequiredService<MongoDbService>().ShiftLogs));

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
