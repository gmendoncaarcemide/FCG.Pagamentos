using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using FCG.Pagamentos.Domain.Pagamentos.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FCG.Pagamentos.API.Endpoints;

public static class ReembolsoEndpoints
{
    public static void MapReembolsoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/reembolsos")
            .WithTags("Reembolsos")
            .WithOpenApi();

        // Endpoints para Reembolsos
        group.MapPost("/", async (IReembolsoService service, [FromBody] CriarReembolsoRequest request) =>
        {
            try
            {
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults);
                }

                var reembolso = await service.CriarAsync(request);
                return Results.Created($"/api/reembolsos/{reembolso.Id}", reembolso);
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
        .WithName("CriarReembolso")
        .WithSummary("Cria um novo reembolso")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (IReembolsoService service, Guid id) =>
        {
            try
            {
                var reembolso = await service.ObterPorIdAsync(id);
                if (reembolso == null)
                    return Results.NotFound();

                return Results.Ok(reembolso);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ObterReembolso")
        .WithSummary("Obtém um reembolso por ID")
        .WithOpenApi();

        group.MapGet("/", async (IReembolsoService service) =>
        {
            try
            {
                var reembolsos = await service.ObterTodosAsync();
                return Results.Ok(reembolsos);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ObterTodosReembolsos")
        .WithSummary("Obtém todos os reembolsos")
        .WithOpenApi();

        group.MapGet("/usuario/{usuarioId:guid}", async (IReembolsoService service, Guid usuarioId) =>
        {
            try
            {
                var reembolsos = await service.ObterPorUsuarioAsync(usuarioId);
                return Results.Ok(reembolsos);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ObterReembolsosPorUsuario")
        .WithSummary("Obtém reembolsos por usuário")
        .WithOpenApi();

        group.MapGet("/transacao/{transacaoId:guid}", async (IReembolsoService service, Guid transacaoId) =>
        {
            try
            {
                var reembolsos = await service.ObterPorTransacaoAsync(transacaoId);
                return Results.Ok(reembolsos);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Erro interno: {ex.Message}");
            }
        })
        .WithName("ObterReembolsosPorTransacao")
        .WithSummary("Obtém reembolsos por transação")
        .WithOpenApi();

        group.MapPut("/{id:guid}/status", async (IReembolsoService service, Guid id, [FromBody] string novoStatus) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(novoStatus))
                    return Results.BadRequest("Status é obrigatório");

                if (!Enum.TryParse<StatusReembolso>(novoStatus, true, out var statusEnum))
                    return Results.BadRequest("Status inválido");

                var reembolso = await service.AtualizarStatusAsync(id, statusEnum);
                return Results.Ok(reembolso);
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
        .WithName("AtualizarStatusReembolso")
        .WithSummary("Atualiza o status de um reembolso")
        .WithOpenApi();

        group.MapPost("/{id:guid}/cancelar", async (IReembolsoService service, Guid id) =>
        {
            try
            {
                await service.CancelarReembolsoAsync(id);
                return Results.NoContent();
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
        .WithName("CancelarReembolso")
        .WithSummary("Cancela um reembolso")
        .WithOpenApi();

        group.MapPost("/{id:guid}/processar", async (IReembolsoService service, Guid id) =>
        {
            try
            {
                var reembolso = await service.ProcessarReembolsoAsync(id);
                return Results.Ok(reembolso);
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
        .WithName("ProcessarReembolso")
        .WithSummary("Processa um reembolso")
        .WithOpenApi();
    }
} 