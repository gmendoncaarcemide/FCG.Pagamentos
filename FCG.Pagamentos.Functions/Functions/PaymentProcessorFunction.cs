using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
using FCG.Pagamentos.Functions.Models;

namespace FCG.Pagamentos.Functions.Functions
{
    public class PaymentProcessorFunction
    {
        private readonly ILogger<PaymentProcessorFunction> _logger;

        public PaymentProcessorFunction(ILogger<PaymentProcessorFunction> logger)
        {
            _logger = logger;
        }

        [Function("PaymentProcessor")]
        public async Task<HttpResponseData> ProcessPayment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "payments/process")] HttpRequestData req)
        {
            _logger.LogInformation("🚀 PaymentProcessorFunction executada - Integração com NotificationProcessor");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("📥 Dados recebidos: {RequestBody}", requestBody);

                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogWarning("⚠️ Corpo da requisição vazio");
                    return await CreateError(req, HttpStatusCode.BadRequest, "Corpo da requisição não pode estar vazio");
                }

                var paymentRequest = JsonSerializer.Deserialize<PaymentProcessRequest>(requestBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (paymentRequest == null)
                {
                    _logger.LogWarning("⚠️ Falha ao deserializar requisição");
                    return await CreateError(req, HttpStatusCode.BadRequest, "Formato da requisição inválido");
                }

                _logger.LogInformation("✅ Dados validados com sucesso");
                _logger.LogInformation("   Usuário: {UsuarioId}", paymentRequest.UsuarioId);
                _logger.LogInformation("   Jogo: {JogoId}", paymentRequest.JogoId);
                _logger.LogInformation("   Valor: R$ {Valor}", paymentRequest.Valor);
                _logger.LogInformation("   Tipo: {TipoPagamento}", paymentRequest.TipoPagamento);

                // Simular processamento
                var transacaoId = Guid.NewGuid();
                var codigoAutorizacao = GerarCodigoAutorizacao((int)paymentRequest.TipoPagamento);

                _logger.LogInformation("💳 Pagamento processado com sucesso!");
                _logger.LogInformation("   Transação: {TransacaoId}", transacaoId);
                _logger.LogInformation("   Código: {CodigoAutorizacao}", codigoAutorizacao);

                // Enviar notificação real para a fila
                await EnviarParaNotificationQueue(transacaoId, paymentRequest.UsuarioId, paymentRequest.Valor, paymentRequest.TipoPagamento);

                var response = new PaymentProcessResponse
                {
                    Status = "Aprovada",
                    Message = "Pagamento processado com sucesso via Azure Function",
                    TransacaoId = transacaoId,
                    CodigoAutorizacao = codigoAutorizacao,
                    ProcessedAt = DateTime.UtcNow
                };

                _logger.LogInformation("✅ Resposta enviada com sucesso");
                return await CreateSuccess(req, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro na PaymentProcessorFunction");
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
                _logger.LogError("InnerException: {Inner}", ex.InnerException?.Message);
                return await CreateError(req, HttpStatusCode.InternalServerError, $"Erro interno: {ex.Message}");
            }
        }

        private async Task EnviarParaNotificationQueue(Guid transacaoId, Guid usuarioId, decimal valor, object tipoPagamento)
        {
            try
            {
                _logger.LogInformation("📦 Enviando mensagem para a fila NotificationProcessor...");

                // Recupera a connection string da Storage Account
                var connectionString = Environment.GetEnvironmentVariable("NotificationQueueConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.LogError("❌ Variável de ambiente 'NotificationQueueConnection' não configurada.");
                    return;
                }

                // Cria cliente da fila
                var queueClient = new QueueClient(connectionString, "notification-queue");
                await queueClient.CreateIfNotExistsAsync();

                var notification = new
                {
                    TransacaoId = transacaoId,
                    UsuarioId = usuarioId,
                    TipoNotificacao = "PagamentoAprovado",
                    Valor = valor,
                    TipoPagamento = tipoPagamento.ToString(),
                    Mensagem = $"Seu pagamento de R$ {valor:F2} foi aprovado com sucesso!",
                    DataEnvio = DateTime.UtcNow
                };

                string json = JsonSerializer.Serialize(notification);
                string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

                await queueClient.SendMessageAsync(base64Message);

                _logger.LogInformation("✅ Mensagem enviada para a fila 'notification-queue' com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao enviar mensagem para a fila NotificationProcessor: {Message}", ex.Message);
            }
        }

        private static string GerarCodigoAutorizacao(int tipoPagamento)
        {
            return tipoPagamento switch
            {
                3 => $"PIX{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                1 => $"CC{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                2 => $"CD{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                4 => $"BOL{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                _ => Guid.NewGuid().ToString("N")[..8].ToUpper()
            };
        }

        private async Task<HttpResponseData> CreateSuccess(HttpRequestData req, PaymentProcessResponse response)
        {
            var res = req.CreateResponse(HttpStatusCode.OK);
            res.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await res.WriteStringAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            return res;
        }

        private async Task<HttpResponseData> CreateError(HttpRequestData req, HttpStatusCode statusCode, string message)
        {
            var res = req.CreateResponse(statusCode);
            res.Headers.Add("Content-Type", "application/json; charset=utf-8");
            var error = new { status = "error", message };
            await res.WriteStringAsync(JsonSerializer.Serialize(error, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            return res;
        }
    }
}
