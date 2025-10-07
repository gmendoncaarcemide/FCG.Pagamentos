using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FCG.Pagamentos.Application.Pagamentos.Services;

public interface IAzureFunctionService
{
    Task EnviarNotificacaoAsync(Guid transacaoId, Guid usuarioId, string status, decimal valor, string tipoPagamento);
    Task ChamarPaymentProcessorFunctionAsync(object paymentData);
}

public class AzureFunctionService : IAzureFunctionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureFunctionService> _logger;
    private readonly IConfiguration _configuration;

    public AzureFunctionService(HttpClient httpClient, ILogger<AzureFunctionService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task EnviarNotificacaoAsync(Guid transacaoId, Guid usuarioId, string status, decimal valor, string tipoPagamento)
    {
        try
        {
            _logger.LogInformation("🔔 Enviando notificação para Azure Function - Transação: {TransacaoId}, Status: {Status}", 
                transacaoId, status);

            var notification = new
            {
                TransacaoId = transacaoId,
                UsuarioId = usuarioId,
                TipoNotificacao = GetTipoNotificacao(status),
                Conteudo = GetConteudoNotificacao(status, valor, tipoPagamento),
                CriadoEm = DateTime.UtcNow,
                DadosAdicionais = new Dictionary<string, object>
                {
                    { "valor", valor },
                    { "tipoPagamento", tipoPagamento },
                    { "status", status }
                }
            };
            _logger.LogInformation("📧 NOTIFICAÇÃO SIMULADA:");
            _logger.LogInformation("   Transação: {TransacaoId}", transacaoId);
            _logger.LogInformation("   Usuário: {UsuarioId}", usuarioId);
            _logger.LogInformation("   Tipo: {TipoNotificacao}", notification.TipoNotificacao);
            _logger.LogInformation("   Conteúdo: {Conteudo}", notification.Conteudo);
            _logger.LogInformation("   Status: ✅ Notificação processada com sucesso!");

            await Task.Delay(100);

            _logger.LogInformation("✅ Notificação enviada com sucesso para Azure Function");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao enviar notificação para Azure Function");
        }
    }

    public async Task ChamarPaymentProcessorFunctionAsync(object paymentData)
    {
        try
        {
            var functionUrl = _configuration["AzureFunctions:PaymentProcessorUrl"];
            var functionKey = _configuration["AzureFunctions:PaymentProcessorKey"];

            if (string.IsNullOrEmpty(functionUrl) || string.IsNullOrEmpty(functionKey))
            {
                _logger.LogWarning("⚠️ Azure Function URL ou Key não configurados. Notificação simulada.");
                await EnviarNotificacaoSimulada(paymentData);
                return;
            }

            _logger.LogInformation("🚀 Chamando PaymentProcessorFunction - URL: {Url}", functionUrl);

            var json = JsonSerializer.Serialize(paymentData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{functionUrl}?code={functionKey}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ PaymentProcessorFunction executada com sucesso: {Response}", responseContent);
            }
            else
            {
                _logger.LogError("❌ Erro na PaymentProcessorFunction: {StatusCode} - {Reason}", 
                    response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao chamar PaymentProcessorFunction");
        }
    }

    private async Task EnviarNotificacaoSimulada(object paymentData)
    {
        _logger.LogInformation("📱 SIMULANDO CHAMADA PARA PAYMENTPROCESSORFUNCTION:");
        _logger.LogInformation("   Dados: {PaymentData}", JsonSerializer.Serialize(paymentData));
        _logger.LogInformation("   Status: ✅ Function executada com sucesso!");
        
        await Task.Delay(50);
    }

    private static string GetTipoNotificacao(string status)
    {
        return status switch
        {
            "Aprovada" => "pagamento_aprovado",
            "Recusada" => "pagamento_recusado",
            "Processando" => "pagamento_pendente",
            _ => "pagamento_processado"
        };
    }

    private static string GetConteudoNotificacao(string status, decimal valor, string tipoPagamento)
    {
        return status switch
        {
            "Aprovada" => $"✅ Pagamento de R$ {valor:F2} via {tipoPagamento} foi aprovado com sucesso!",
            "Recusada" => $"❌ Pagamento de R$ {valor:F2} via {tipoPagamento} foi recusado. Verifique seus dados.",
            "Processando" => $"⏳ Pagamento de R$ {valor:F2} via {tipoPagamento} está sendo processado...",
            _ => $"📋 Pagamento de R$ {valor:F2} via {tipoPagamento} - Status: {status}"
        };
    }
}
