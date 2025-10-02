using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Functions.Models;

namespace FCG.Pagamentos.Functions.Functions;

public class NotificationFunction
{
    private readonly ILogger<NotificationFunction> _logger;
    private readonly ITransacaoService _transacaoService;

    public NotificationFunction(ILogger<NotificationFunction> logger, ITransacaoService transacaoService)
    {
        _logger = logger;
        _transacaoService = transacaoService;
    }

    [Function("NotificationProcessor")]
    public async Task ProcessNotification(
        [QueueTrigger("notification-queue", Connection = "NotificationQueueConnection")] string queueMessage)
    {
        _logger.LogInformation("Mensagem recebida: {Message}", queueMessage);

        try
        {
            var notification = JsonSerializer.Deserialize<NotificationMessage>(queueMessage,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (notification == null)
            {
                _logger.LogWarning("Falha ao deserializar: {Message}", queueMessage);
                return;
            }

            _logger.LogInformation("Notificação: {Tipo}, Transação={TransacaoId}, Usuário={UsuarioId}",
                notification.TipoNotificacao, notification.TransacaoId, notification.UsuarioId);

            await ProcessNotificationAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar notificação");
            throw;
        }
    }

    private async Task ProcessNotificationAsync(NotificationMessage notification)
    {
        if (notification.TransacaoId != Guid.Empty)
        {
            try
            {
                var transacao = await _transacaoService.ObterPorIdAsync(notification.TransacaoId);
                if (transacao != null)
                {
                    _logger.LogInformation("Transação: Status={Status}, Valor={Valor}", transacao.Status, transacao.Valor);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar transação {TransacaoId}", notification.TransacaoId);
            }
        }

        await ProcessByType(notification);
    }

    private async Task ProcessByType(NotificationMessage notification)
    {
        var tipo = notification.TipoNotificacao.ToLower();

        if (tipo == "pagamento_aprovado") await Send(notification, "Email de confirmação", "Push");
        else if (tipo == "pagamento_recusado") await Send(notification, "Email de recusado", "Suporte");
        else if (tipo == "pagamento_pendente") await Send(notification, "Email de pendente");
        else if (tipo == "sistema_manutencao") await Send(notification, "Email de manutenção", "Push broadcast");
        else await Send(notification, $"Notificação: {notification.TipoNotificacao}");
    }

    private async Task Send(NotificationMessage notification, params string[] canais)
    {
        foreach (var canal in canais)
        {
            _logger.LogInformation("Enviando via {Canal}: '{Conteudo}' para {UsuarioId}",
                canal, notification.Conteudo, notification.UsuarioId);

            await Task.Delay(20);
        }

        _logger.LogInformation("Notificação {Tipo} enviada com sucesso", notification.TipoNotificacao);
    }
}
