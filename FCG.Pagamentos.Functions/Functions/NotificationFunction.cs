using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Functions.Models;

namespace FCG.Pagamentos.Functions.Functions;

/// <summary>
/// Azure Function para processamento de notificações via fila
/// 
/// Configuração no Azure:
/// - Tipo: Queue Trigger
/// - Fila: notification-queue
/// - Connection String: NotificationQueueConnection (configurar no Application Settings)
/// - Configurar Storage Account com fila "notification-queue"
/// 
/// Para desenvolvimento local:
/// - Configurar ConnectionString "NotificationQueueConnection" no local.settings.json
/// - Usar Azurite (emulador local do Azure Storage) ou Azure Storage Account real
/// - Executar com: func start (após instalar Azure Functions Core Tools)
/// 
/// Como enviar mensagem para a fila:
/// - Via código: use QueueClient do Azure.Storage.Queues
/// - Via portal: Azure Storage Explorer ou Portal do Azure
/// - Exemplo de mensagem JSON:
/// {
///   "transacaoId": "guid",
///   "usuarioId": "guid", 
///   "tipoNotificacao": "pagamento_aprovado",
///   "conteudo": "Seu pagamento foi aprovado com sucesso",
///   "dadosAdicionais": { "valor": 50.00, "jogo": "Game XYZ" }
/// }
/// </summary>
public class NotificationFunction
{
    private readonly ILogger<NotificationFunction> _logger;
    private readonly ITransacaoService _transacaoService;

    public NotificationFunction(
        ILogger<NotificationFunction> logger,
        ITransacaoService transacaoService)
    {
        _logger = logger;
        _transacaoService = transacaoService;
    }

    [Function("NotificationProcessor")]
    public async Task ProcessNotification(
        [QueueTrigger("notification-queue", Connection = "NotificationQueueConnection")] string queueMessage)
    {
        _logger.LogInformation("Processando mensagem da fila de notificação: {Message}", queueMessage);

        try
        {
            // Deserializa a mensagem da fila
            var notification = JsonSerializer.Deserialize<NotificationMessage>(queueMessage,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (notification == null)
            {
                _logger.LogWarning("Falha ao deserializar mensagem da fila: {Message}", queueMessage);
                return;
            }

            _logger.LogInformation("Processando notificação: Tipo={TipoNotificacao}, TransacaoId={TransacaoId}, UsuarioId={UsuarioId}",
                notification.TipoNotificacao, notification.TransacaoId, notification.UsuarioId);

            // Simula o processamento da notificação
            await ProcessNotificationAsync(notification);

            _logger.LogInformation("Notificação processada com sucesso para transação {TransacaoId}", 
                notification.TransacaoId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar mensagem da fila: {Message}", queueMessage);
            // Em produção, você pode querer enviar para dead letter queue
            throw; // Re-throw para tentar novamente (até maxDequeueCount)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar notificação: {Message}", queueMessage);
            // Em produção, considere implementar retry policy ou dead letter queue
            throw; // Re-throw para tentar novamente (até maxDequeueCount)
        }
    }

    /// <summary>
    /// Processa a notificação específica
    /// </summary>
    private async Task ProcessNotificationAsync(NotificationMessage notification)
    {
        // Valida se a transação existe (opcional - para notificações relacionadas a transações)
        if (notification.TransacaoId != Guid.Empty)
        {
            try
            {
                var transacao = await _transacaoService.ObterPorIdAsync(notification.TransacaoId);
                if (transacao == null)
                {
                    _logger.LogWarning("Transação {TransacaoId} não encontrada para notificação", 
                        notification.TransacaoId);
                    // Continua o processamento mesmo assim - pode ser notificação não relacionada a transação
                }
                else
                {
                    _logger.LogInformation("Transação encontrada: Status={Status}, Valor={Valor}", 
                        transacao.Status, transacao.Valor);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar transação {TransacaoId}", notification.TransacaoId);
                // Continua o processamento
            }
        }

        // Simula diferentes tipos de notificação
        await ProcessByNotificationType(notification);
    }

    /// <summary>
    /// Processa diferentes tipos de notificação
    /// </summary>
    private async Task ProcessByNotificationType(NotificationMessage notification)
    {
        switch (notification.TipoNotificacao.ToLower())
        {
            case "pagamento_aprovado":
                await ProcessPaymentApprovedNotification(notification);
                break;

            case "pagamento_recusado":
                await ProcessPaymentRejectedNotification(notification);
                break;

            case "pagamento_pendente":
                await ProcessPaymentPendingNotification(notification);
                break;

            case "sistema_manutencao":
                await ProcessSystemMaintenanceNotification(notification);
                break;

            default:
                await ProcessGenericNotification(notification);
                break;
        }
    }

    /// <summary>
    /// Processa notificação de pagamento aprovado
    /// </summary>
    private async Task ProcessPaymentApprovedNotification(NotificationMessage notification)
    {
        _logger.LogInformation("Enviando notificação de pagamento aprovado para usuário {UsuarioId}", 
            notification.UsuarioId);

        // Aqui você implementaria a integração real com:
        // - Serviço de email (SendGrid, Azure Communication Services)
        // - Push notifications (Firebase, Azure Notification Hubs)  
        // - SMS (Twilio, Azure Communication Services)
        // - Webhook para sistemas externos

        await SimulateNotificationSending(notification, "Email de confirmação de pagamento");
        await SimulateNotificationSending(notification, "Push notification");

        // Simula atualização de logs ou auditoria
        _logger.LogInformation("Notificação de pagamento aprovado processada com sucesso");
        await Task.Delay(100); // Simula tempo de processamento
    }

    /// <summary>
    /// Processa notificação de pagamento recusado
    /// </summary>
    private async Task ProcessPaymentRejectedNotification(NotificationMessage notification)
    {
        _logger.LogInformation("Enviando notificação de pagamento recusado para usuário {UsuarioId}", 
            notification.UsuarioId);

        await SimulateNotificationSending(notification, "Email de pagamento recusado");
        
        // Para pagamentos recusados, pode ser útil também notificar canais internos
        _logger.LogInformation("Notificando equipe de suporte sobre pagamento recusado");
        
        await Task.Delay(100);
    }

    /// <summary>
    /// Processa notificação de pagamento pendente
    /// </summary>
    private async Task ProcessPaymentPendingNotification(NotificationMessage notification)
    {
        _logger.LogInformation("Enviando notificação de pagamento pendente para usuário {UsuarioId}", 
            notification.UsuarioId);

        await SimulateNotificationSending(notification, "Email de pagamento pendente");
        
        await Task.Delay(100);
    }

    /// <summary>
    /// Processa notificação de manutenção do sistema
    /// </summary>
    private async Task ProcessSystemMaintenanceNotification(NotificationMessage notification)
    {
        _logger.LogInformation("Processando notificação de manutenção do sistema");

        // Para notificações de sistema, pode ser broadcast para todos os usuários
        await SimulateNotificationSending(notification, "Email de manutenção programada");
        await SimulateNotificationSending(notification, "Push notification para todos os usuários");

        await Task.Delay(100);
    }

    /// <summary>
    /// Processa notificação genérica
    /// </summary>
    private async Task ProcessGenericNotification(NotificationMessage notification)
    {
        _logger.LogInformation("Processando notificação genérica: {TipoNotificacao}", 
            notification.TipoNotificacao);

        await SimulateNotificationSending(notification, $"Notificação: {notification.TipoNotificacao}");
        
        await Task.Delay(50);
    }

    /// <summary>
    /// Simula o envio de notificação para diferentes canais
    /// </summary>
    private async Task SimulateNotificationSending(NotificationMessage notification, string channel)
    {
        _logger.LogInformation("Enviando via {Channel}: '{Conteudo}' para usuário {UsuarioId}",
            channel, notification.Conteudo, notification.UsuarioId);

        // Em uma implementação real, aqui você faria:
        // - Chamadas para APIs de terceiros (SendGrid, Firebase, etc.)
        // - Salvar no banco de dados para auditoria
        // - Integrar com sistemas de CRM
        // - Enviar webhooks para sistemas externos

        // Simula tempo de envio
        await Task.Delay(20);

        _logger.LogInformation("✅ {Channel} enviado com sucesso", channel);
    }
}
