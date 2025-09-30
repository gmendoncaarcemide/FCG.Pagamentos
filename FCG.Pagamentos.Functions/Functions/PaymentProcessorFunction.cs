using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using FCG.Pagamentos.Functions.Models;
using FCG.Pagamentos.Domain.Pagamentos.Entities;

namespace FCG.Pagamentos.Functions.Functions;

/// <summary>
/// Azure Function para processamento de pagamentos
/// 
/// Configuração no Azure:
/// - Tipo: HTTP Trigger
/// - Rota: POST /payments/process
/// - Authentication Level: Function (usar Function Key para segurança)
/// - Configurar Connection String "DefaultConnection" no Application Settings
/// 
/// Para desenvolvimento local:
/// - Configurar ConnectionString no local.settings.json
/// - Executar com: func start (após instalar Azure Functions Core Tools)
/// </summary>
public class PaymentProcessorFunction
{
    private readonly ILogger<PaymentProcessorFunction> _logger;
    private readonly ITransacaoService _transacaoService;

    public PaymentProcessorFunction(
        ILogger<PaymentProcessorFunction> logger,
        ITransacaoService transacaoService)
    {
        _logger = logger;
        _transacaoService = transacaoService;
    }

    [Function("PaymentProcessor")]
    public async Task<HttpResponseData> ProcessPayment(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "payments/process")] HttpRequestData req)
    {
        _logger.LogInformation("Iniciando processamento de pagamento via Azure Function");

        try
        {
            // Lê o corpo da requisição
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Corpo da requisição está vazio");
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    "Corpo da requisição não pode estar vazio");
            }

            // Deserializa o JSON
            var paymentRequest = JsonSerializer.Deserialize<PaymentProcessRequest>(requestBody, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (paymentRequest == null)
            {
                _logger.LogWarning("Falha ao deserializar a requisição");
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    "Formato da requisição inválido");
            }

            // Valida o modelo
            var validationContext = new ValidationContext(paymentRequest);
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(paymentRequest, validationContext, validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
                _logger.LogWarning("Validação falhou: {Errors}", errorMessage);
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    $"Dados inválidos: {errorMessage}");
            }

            // Converte para o modelo da aplicação
            var criarTransacaoRequest = new CriarTransacaoRequest
            {
                UsuarioId = paymentRequest.UsuarioId,
                JogoId = paymentRequest.JogoId,
                Valor = paymentRequest.Valor,
                TipoPagamento = paymentRequest.TipoPagamento,
                Observacoes = paymentRequest.Observacoes
            };

            // TODO: Aqui seria onde deserializaríamos os dados específicos de pagamento
            // Por simplicidade, mantendo apenas os campos básicos por enquanto

            _logger.LogInformation("Processando transação para usuário {UsuarioId}, jogo {JogoId}, valor {Valor}", 
                paymentRequest.UsuarioId, paymentRequest.JogoId, paymentRequest.Valor);

            // Processa o pagamento usando o serviço existente
            var transacao = await _transacaoService.CriarAsync(criarTransacaoRequest);

            _logger.LogInformation("Transação processada com sucesso. ID: {TransacaoId}, Status: {Status}", 
                transacao.Id, transacao.Status);

            // Cria resposta de sucesso
            var response = new PaymentProcessResponse
            {
                Status = transacao.Status.ToString(),
                Message = "Pagamento processado com sucesso",
                TransacaoId = transacao.Id,
                CodigoAutorizacao = transacao.CodigoAutorizacao
            };

            return await CreateSuccessResponse(req, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro de negócio durante processamento do pagamento");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno durante processamento do pagamento");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, 
                "Erro interno do servidor. Tente novamente mais tarde.");
        }
    }

    /// <summary>
    /// Cria uma resposta de sucesso
    /// </summary>
    private async Task<HttpResponseData> CreateSuccessResponse(HttpRequestData req, PaymentProcessResponse response)
    {
        var httpResponse = req.CreateResponse(HttpStatusCode.OK);
        httpResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
        
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        
        await httpResponse.WriteStringAsync(jsonResponse);
        return httpResponse;
    }

    /// <summary>
    /// Cria uma resposta de erro
    /// </summary>
    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        var httpResponse = req.CreateResponse(statusCode);
        httpResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
        
        var errorResponse = new { status = "error", message };
        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        
        await httpResponse.WriteStringAsync(jsonResponse);
        return httpResponse;
    }
}
