namespace CheckoutService.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(string routingKey, object message, string? messageType = null, string? correlationId = null);
}