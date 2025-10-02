using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using FCG.Pagamentos.Domain.Pagamentos.Entities;
using FCG.Pagamentos.Functions.Models;

namespace FCG.Pagamentos.Functions.Functions;

public class PaymentProcessorFunction
{
    private readonly ILogger<PaymentProcessorFunction> _logger;
    private readonly ITransacaoService _transacaoService;

    public PaymentProcessorFunction(ILogger<PaymentProcessorFunction> logger, ITransacaoService transacaoService)
    {
        _logger = logger;
        _transacaoService = transacaoService;
    }

    [Function("PaymentProcessor")]
    public async Task<HttpResponseData> ProcessPayment(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "payments/process")] HttpRequestData req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        if (string.IsNullOrEmpty(requestBody))
            return await CreateError(req, HttpStatusCode.BadRequest, "Corpo da requisição não pode estar vazio");

        var paymentRequest = JsonSerializer.Deserialize<PaymentProcessRequest>(requestBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (paymentRequest == null)
            return await CreateError(req, HttpStatusCode.BadRequest, "Formato da requisição inválido");

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(paymentRequest, new ValidationContext(paymentRequest), validationResults, true))
        {
            var errors = string.Join(", ", validationResults.Select(v => v.ErrorMessage));
            return await CreateError(req, HttpStatusCode.BadRequest, $"Dados inválidos: {errors}");
        }

        var criarTransacaoRequest = new CriarTransacaoRequest
        {
            UsuarioId = paymentRequest.UsuarioId,
            JogoId = paymentRequest.JogoId,
            Valor = paymentRequest.Valor,
            TipoPagamento = paymentRequest.TipoPagamento,
            Observacoes = paymentRequest.Observacoes,
            DadosCartao = paymentRequest.DadosCartao != null ? new Application.Pagamentos.ViewModels.DadosCartaoRequest
            {
                NumeroCartao = paymentRequest.DadosCartao.NumeroCartao,
                NomeTitular = paymentRequest.DadosCartao.NomeTitular,
                DataValidade = paymentRequest.DadosCartao.DataValidade,
                CVV = paymentRequest.DadosCartao.CVV,
                Parcelas = paymentRequest.DadosCartao.Parcelas
            } : null,
            DadosPIX = paymentRequest.DadosPIX != null ? new Application.Pagamentos.ViewModels.DadosPIXRequest
            {
                ChavePIX = paymentRequest.DadosPIX.ChavePIX
            } : null,
            DadosBoleto = paymentRequest.DadosBoleto != null ? new Application.Pagamentos.ViewModels.DadosBoletoRequest
            {
                CpfCnpj = paymentRequest.DadosBoleto.CpfCnpj,
                NomePagador = paymentRequest.DadosBoleto.NomePagador,
                Endereco = paymentRequest.DadosBoleto.Endereco,
                CEP = paymentRequest.DadosBoleto.CEP,
                Cidade = paymentRequest.DadosBoleto.Cidade,
                Estado = paymentRequest.DadosBoleto.Estado
            } : null
        };

        try
        {
            var transacao = await _transacaoService.CriarAsync(criarTransacaoRequest);

            var response = new PaymentProcessResponse
            {
                Status = transacao.Status.ToString(),
                Message = "Pagamento processado com sucesso",
                TransacaoId = transacao.Id,
                CodigoAutorizacao = transacao.CodigoAutorizacao
            };

            return await CreateSuccess(req, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro de negócio");
            return await CreateError(req, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno");
            return await CreateError(req, HttpStatusCode.InternalServerError, "Erro interno do servidor.");
        }
    }

    private async Task<HttpResponseData> CreateSuccess(HttpRequestData req, PaymentProcessResponse response)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);
        res.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await res.WriteStringAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        return res;
    }

    private async Task<HttpResponseData> CreateError(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        var res = req.CreateResponse(statusCode);
        res.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var error = new { status = "error", message };
        await res.WriteStringAsync(JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        return res;
    }
}
