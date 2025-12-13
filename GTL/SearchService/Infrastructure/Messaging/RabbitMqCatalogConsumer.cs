using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SearchService.Infrastructure.Data;

namespace SearchService.Infrastructure.Messaging;

public sealed class RabbitMqCatalogConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IChannel? _channel;

    private const string ExchangeName = "gtl.catalog";
    private const string QueueName = "gtl.search.bookcreated";

    public RabbitMqCatalogConsumer(IServiceProvider services, IConfiguration config)
    {
        _services = services;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMq:Host"] ?? "localhost",
            UserName = _config["RabbitMq:User"] ?? "gtl",
            Password = _config["RabbitMq:Pass"] ?? "gtlpassword",
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, routingKey: "book.created", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            // Fix for ambiguous Encoding.GetString overloads:
            var json = Encoding.UTF8.GetString(ea.Body.Span);

            var msg = JsonSerializer.Deserialize<BookCreatedMessage>(json);
            if (msg is null) return;

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SearchDbContext>();

            db.SearchDocuments.Add(new SearchDocument
            {
                BookId = msg.Id,
                Title = msg.Title,
                Author = msg.Author,
                Isbn = msg.Isbn
            });

            await db.SaveChangesAsync(stoppingToken);
        };

        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

        // Keep the background service alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private record BookCreatedMessage(int Id, string Isbn, string Title, string Author);
}
