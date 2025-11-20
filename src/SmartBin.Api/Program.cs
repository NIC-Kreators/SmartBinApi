using MQTTnet.AspNetCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var kestrelHttpUri = new Uri(builder.Configuration.GetValue<string>("Kestrel:Endpoints:Http:Url", "http://localhost:8080"));

var httpPort = kestrelHttpUri.Port;
var mqttPort = builder.Configuration.GetValue("Mqtt:Port", 1883);

builder.Services
    .AddHostedMqttServer(mqttServer => mqttServer
                             .WithDefaultEndpoint()
                             .WithDefaultEndpointPort(mqttPort))
    .AddMqttConnectionHandler()
    .AddConnections();

builder.WebHost.ConfigureKestrel(webHostOptions =>
{
    webHostOptions.ListenAnyIP(httpPort);
    webHostOptions.ListenAnyIP(mqttPort, mqttOptions => mqttOptions.UseMqtt());
});

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

app.UseRouting();
app.MapGet("/hello", () => "Hello").Stable();

app.UseEndpoints(endpoints =>
{
    endpoints.MapConnectionHandler<MqttConnectionHandler>(
        "/mqtt",
        options => options.WebSockets.SubProtocolSelector = 
            protocolList => protocolList.FirstOrDefault() ?? string.Empty);
});

app.UseMqttServer(server =>
{
    server.ValidatingConnectionAsync += e =>
    {
        Console.WriteLine($"Client '{e.ClientId}' connecting");
        return Task.CompletedTask;
    };

    server.ClientConnectedAsync += e =>
    {
        Console.WriteLine($"Client '{e.ClientId}' connected");
        return Task.CompletedTask;
    };

    server.InterceptingPublishAsync += args =>
    {
        Console.WriteLine($"Message from {args.ApplicationMessage.Topic} received!");
        // TODO Handle mqtt logic
        return Task.CompletedTask;
    };
});

app.Run();
