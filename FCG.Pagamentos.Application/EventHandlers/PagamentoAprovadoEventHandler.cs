using FCG.Pagamentos.Application.Messaging.Events;
using FCG.Pagamentos.Application.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG.Pagamentos.Application.EventHandlers;

public class PagamentoAprovadoEventHandler : IEventHandler<PagamentoAprovadoEvent>
{
    private readonly ILogger<PagamentoAprovadoEventHandler> _logger;
    private readonly IEventBus _eventBus;

    public PagamentoAprovadoEventHandler(
        ILogger<PagamentoAprovadoEventHandler> logger,
        IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task HandleAsync(PagamentoAprovadoEvent @event)
    {
        _logger.LogInformation(
            "Processing PagamentoAprovadoEvent for Transaction {TransacaoId}, User {UsuarioId}, Game {JogoId}",
            @event.TransacaoId, @event.UsuarioId, @event.JogoId);

        var notificacao = new NotificacaoEvent
        {
            UsuarioId = @event.UsuarioId,
            Titulo = "Pagamento Aprovado",
            Mensagem = $"Seu pagamento no valor de R$ {@event.Valor:F2} foi aprovado com sucesso!",
            Tipo = TipoNotificacao.PagamentoAprovado,
            Metadata = new Dictionary<string, string>
            {
                { "TransacaoId", @event.TransacaoId.ToString() },
                { "JogoId", @event.JogoId.ToString() },
                { "CodigoAutorizacao", @event.CodigoAutorizacao }
            }
        };

        await _eventBus.PublishAsync(notificacao);

        _logger.LogInformation(
            "Notification sent for approved payment {TransacaoId}",
            @event.TransacaoId);
    }
}
