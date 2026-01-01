using FCG.Pagamentos.Application.Messaging.Events;

namespace FCG.Pagamentos.Application.Messaging.Interfaces;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, string? routingKey = null) where TEvent : IntegrationEvent;
    void Subscribe<TEvent, THandler>() 
        where TEvent : IntegrationEvent 
        where THandler : IEventHandler<TEvent>;
}
