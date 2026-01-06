using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace CheckoutService.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly string _exchangeName;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMqEventPublisher(string exchangeName, IConnection connection, IChannel channel)
    {
        _exchangeName = exchangeName;
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqEventPublisher> CreateAsync(IConfiguration config)
    {
        var exchangeName = config["RabbitMq:Exchange"] ?? "gtl.checkout";

        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMq:Host"] ?? "localhost",
            Port = int.TryParse(config["RabbitMq:Port"], out var p) ? p : 5672,
            UserName = config["RabbitMq:User"] ?? "gtl",
            Password = config["RabbitMq:Pass"] ?? "gtlpassword"
        };

        var connection = await factory.CreateConnectionAsync();                // v7 API
        var channel = await connection.CreateChannelAsync();                   // v7 API

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true); // v7 API

        return new RabbitMqEventPublisher(exchangeName, connection, channel);
    }

    public async Task PublishAsync(string routingKey, object message, string? messageType = null, string? correlationId = null)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // v7: properties are usually created with 'new BasicProperties()'
        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = messageType,
            CorrelationId = correlationId,
            MessageId = Guid.NewGuid().ToString("N")
        };

        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body
        );
    }

    public async ValueTask DisposeAsync()
    {
        try { await _channel.CloseAsync(); } catch { }
        try { await _connection.CloseAsync(); } catch { }

        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
