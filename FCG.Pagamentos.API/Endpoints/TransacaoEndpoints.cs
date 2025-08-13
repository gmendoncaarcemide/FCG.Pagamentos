using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FCG.Pagamentos.API.Endpoints;

public static class TransacaoEndpoints
{
    public static void MapTransacaoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/transacoes")
            .WithTags("Transações")
            .WithOpenApi();

        // Endpoints para Transações
        group.MapPost("/", async (ITransacaoService service, [FromBody] CriarTransacaoRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var transacao = await service.CriarAsync(request);
                return Results.Created($"/api/transacoes/{transacao.Id}", transacao);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("CriarTransacao")
        .WithSummary("Cria uma nova transação")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (ITransacaoService service, Guid id) =>
        {
            try
            {
                var transacao = await service.ObterPorIdAsync(id);
                if (transacao == null)
                    return Results.NotFound();

                return Results.Ok(transacao);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ObterTransacao")
        .WithSummary("Obtém uma transação por ID")
        .WithOpenApi();

        group.MapGet("/", async (ITransacaoService service) =>
        {
            try
            {
                var transacoes = await service.ObterTodosAsync();
                return Results.Ok(transacoes);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ObterTodasTransacoes")
        .WithSummary("Obtém todas as transações")
        .WithOpenApi();

        group.MapGet("/usuario/{usuarioId:guid}", async (ITransacaoService service, Guid usuarioId) =>
        {
            try
            {
                var transacoes = await service.ObterPorIdAsync(usuarioId);
                return Results.Ok(transacoes);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ObterTransacoesPorUsuario")
        .WithSummary("Obtém transações por usuário")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (ITransacaoService service, Guid id, [FromBody] AtualizarTransacaoRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var transacao = await service.AtualizarAsync(id, request);
                return Results.Ok(transacao);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("AtualizarTransacao")
        .WithSummary("Atualiza uma transação")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (ITransacaoService service, Guid id) =>
        {
            try
            {
                await service.ExcluirAsync(id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ExcluirTransacao")
        .WithSummary("Exclui uma transação")
        .WithOpenApi();

        group.MapPost("/processar", async (ITransacaoService service, [FromBody] ProcessarPagamentoRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var transacao = await service.ProcessarPagamentoAsync(request);
                return Results.Ok(transacao);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ProcessarPagamento")
        .WithSummary("Processa um pagamento")
        .WithOpenApi();

        group.MapPost("/{id:guid}/cancelar", async (ITransacaoService service, Guid id) =>
        {
            try
            {
                var transacao = await service.CancelarTransacaoAsync(id);
                return Results.Ok(transacao);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("CancelarTransacao")
        .WithSummary("Cancela uma transação")
        .WithOpenApi();

        group.MapPost("/buscar", async (ITransacaoService service, [FromBody] BuscarTransacoesRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var transacoes = await service.BuscarAsync(request);
                return Results.Ok(transacoes);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("BuscarTransacoes")
        .WithSummary("Busca transações com filtros")
        .WithOpenApi();

        group.MapGet("/referencia/{referencia}", async (ITransacaoService service, string referencia) =>
        {
            try
            {
                var transacao = await service.ObterPorReferenciaAsync(referencia);
                if (transacao == null)
                    return Results.NotFound();

                return Results.Ok(transacao);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ObterTransacaoPorReferencia")
        .WithSummary("Obtém uma transação por referência")
        .WithOpenApi();
    }
} 