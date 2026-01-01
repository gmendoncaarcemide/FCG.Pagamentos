using FCG.Pagamentos.Application.Messaging.Events;
using FCG.Pagamentos.Application.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG.Pagamentos.Application.EventHandlers;

public class PagamentoRecusadoEventHandler : IEventHandler<PagamentoRecusadoEvent>
{
    private readonly ILogger<PagamentoRecusadoEventHandler> _logger;
    private readonly IEventBus _eventBus;

    public PagamentoRecusadoEventHandler(
        ILogger<PagamentoRecusadoEventHandler> logger,
        IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task HandleAsync(PagamentoRecusadoEvent @event)
    {
        _logger.LogInformation(
            "Processing PagamentoRecusadoEvent for Transaction {TransacaoId}, User {UsuarioId}",
            @event.TransacaoId, @event.UsuarioId);

        var notificacao = new NotificacaoEvent
        {
            UsuarioId = @event.UsuarioId,
            Titulo = "Pagamento Recusado",
            Mensagem = $"Seu pagamento foi recusado. Motivo: {@event.Motivo}",
            Tipo = TipoNotificacao.PagamentoRecusado,
            Metadata = new Dictionary<string, string>
            {
                { "TransacaoId", @event.TransacaoId.ToString() },
                { "JogoId", @event.JogoId.ToString() },
                { "Motivo", @event.Motivo }
            }
        };

        await _eventBus.PublishAsync(notificacao);

        _logger.LogInformation(
            "Notification sent for refused payment {TransacaoId}",
            @event.TransacaoId);
    }
}
