using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using FCG.Pagamentos.Domain.Pagamentos.Entities;

namespace FCG.Pagamentos.Application.Pagamentos.Interfaces;

public interface IReembolsoService
{
    Task<ReembolsoResponse> CriarAsync(CriarReembolsoRequest request);
    Task<ReembolsoResponse?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<ReembolsoResponse>> ObterTodosAsync();
    Task<ReembolsoResponse> AtualizarStatusAsync(Guid id, StatusReembolso status);
    Task CancelarReembolsoAsync(Guid id);
    Task<ReembolsoResponse> ProcessarReembolsoAsync(Guid id);
    Task<IEnumerable<ReembolsoResponse>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<ReembolsoResponse>> ObterPorTransacaoAsync(Guid transacaoId);
    Task<IEnumerable<ReembolsoResponse>> ObterPorStatusAsync(StatusReembolso status);
    Task<IEnumerable<ReembolsoResponse>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
} 