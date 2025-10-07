using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.Pagamentos.Functions.Functions
{
    public class NotificationProcessorFunction
    {
        private readonly ILogger<NotificationProcessorFunction> _logger;

        public NotificationProcessorFunction(ILogger<NotificationProcessorFunction> logger)
        {
            _logger = logger;
        }

        [Function("NotificationProcessor")]
        public async Task Run(
            [QueueTrigger("notification-queue", Connection = "NotificationQueueConnection")] string queueMessage)
        {
            _logger.LogInformation("🔔 NotificationProcessorFunction acionada pela fila");

            try
            {
                string decodedMessage;
                try
                {
                    var bytes = Convert.FromBase64String(queueMessage);
                    decodedMessage = Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    decodedMessage = queueMessage;
                }

                _logger.LogInformation("📦 Mensagem recebida da fila: {DecodedMessage}", decodedMessage);

                // Desserializa o conteúdo JSON
                var notification = JsonSerializer.Deserialize<NotificationMessage>(decodedMessage,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (notification == null)
                {
                    _logger.LogWarning("⚠️ Falha ao desserializar a mensagem recebida da fila.");
                    return;
                }

                // Log dos dados da notificação
                _logger.LogInformation("✅ Notificação processada com sucesso");
                _logger.LogInformation("   Transação: {TransacaoId}", notification.TransacaoId);
                _logger.LogInformation("   Usuário: {UsuarioId}", notification.UsuarioId);
                _logger.LogInformation("   Tipo: {TipoNotificacao}", notification.TipoNotificacao);
                _logger.LogInformation("   Tipo Pagamento: {TipoPagamento}", notification.TipoPagamento);
                _logger.LogInformation("   Valor: R$ {Valor}", notification.Valor);
                _logger.LogInformation("   Mensagem: {Mensagem}", notification.Mensagem);
                _logger.LogInformation("   Data de Envio: {DataEnvio}", notification.DataEnvio);

                // Simula o envio de e-mail
                await SimularEnvioEmail(notification);

                // Simula o envio de push notification
                await SimularEnvioPush(notification);

                _logger.LogInformation("✅ NotificationProcessor finalizada com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar mensagem da fila: {Message}", ex.Message);
            }
        }

        private async Task SimularEnvioEmail(NotificationMessage notification)
        {
            _logger.LogInformation("📧 SIMULANDO ENVIO DE EMAIL:");
            _logger.LogInformation("   Para: usuario{0}@exemplo.com", notification.UsuarioId.ToString()[..8]);
            _logger.LogInformation("   Assunto: ✅ {0}", notification.TipoNotificacao);
            _logger.LogInformation("   Conteúdo: {0}", notification.Mensagem);
            await Task.Delay(100); // Simula latência
            _logger.LogInformation("   Status: ✅ Email enviado com sucesso!");
        }

        private async Task SimularEnvioPush(NotificationMessage notification)
        {
            _logger.LogInformation("📱 SIMULANDO PUSH NOTIFICATION:");
            _logger.LogInformation("   Para usuário: {0}", notification.UsuarioId);
            _logger.LogInformation("   Mensagem: {0}", notification.Mensagem);
            await Task.Delay(100);
            _logger.LogInformation("   Status: ✅ Push enviada com sucesso!");
        }

        private class NotificationMessage
        {
            public Guid TransacaoId { get; set; }
            public Guid UsuarioId { get; set; }
            public string TipoNotificacao { get; set; } = string.Empty;
            public string TipoPagamento { get; set; } = string.Empty;
            public decimal Valor { get; set; }
            public string Mensagem { get; set; } = string.Empty;
            public DateTime DataEnvio { get; set; }
        }
    }
}
