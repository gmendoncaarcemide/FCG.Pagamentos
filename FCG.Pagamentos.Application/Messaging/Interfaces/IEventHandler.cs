using FCG.Pagamentos.Application.Messaging.Events;

namespace FCG.Pagamentos.Application.Messaging.Interfaces;

public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event);
}
