using System.Text;
using MQTTnet;

namespace SmartBin.Api.Mqtt;

public class MqttClientService : BackgroundService
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly MqttClientSubscribeOptions _subscribeOptions;

    public MqttClientService(IConfiguration config)
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(config.GetValue<string>("MQTT_HOST"), config.GetValue<int>("MQTT_PORT"))
            .WithClientId(config.GetValue<string>("MQTT_CLIENT_ID"));
        
        var isMqttAllowedAnonymous = config.GetValue<bool>("MQTT_ALLOW_ANONYMOUS");
        if (!isMqttAllowedAnonymous)
            optionsBuilder = optionsBuilder.WithCredentials(config.GetValue<string>("MQTT_USERNAME"), config.GetValue<string>("MQTT_PASSWORD"));
        
        _options = optionsBuilder.Build();
        
        _client.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            
            // TODO implement routing
            Console.WriteLine($"Received message on topic '{topic}': {payload}");
            return Task.CompletedTask;
        };

        var subscribeOptionsFilter = new MqttClientSubscribeOptionsBuilder();
        var topics = config.GetSection("Mqtt:Topics").Get<string[]>() ?? [];

        foreach (var topic in topics)
            subscribeOptionsFilter.WithTopicFilter(topic);
        
        _subscribeOptions = subscribeOptionsFilter.Build();
    }
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _client.ConnectAsync(_options, ct);
        await _client.SubscribeAsync(_subscribeOptions, ct);
        
        // Keep process alive for the lifetime of the server
        while (!ct.IsCancellationRequested)
            await Task.Delay(1000, ct);
        
        // ReSharper disable once MethodSupportsCancellation
        await _client.DisconnectAsync();
    }
}
