namespace CatalogService.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(string routingKey, object message);

}