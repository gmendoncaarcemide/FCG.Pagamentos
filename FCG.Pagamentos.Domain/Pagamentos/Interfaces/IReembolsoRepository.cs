using FCG.Pagamentos.Domain.Pagamentos.Entities;

namespace FCG.Pagamentos.Domain.Pagamentos.Interfaces;

public interface IReembolsoRepository
{
    Task<Reembolso?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Reembolso>> ObterTodosAsync();
    Task<IEnumerable<Reembolso>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<Reembolso>> ObterPorTransacaoAsync(Guid transacaoId);
    Task<IEnumerable<Reembolso>> ObterPorStatusAsync(StatusReembolso status);
    Task<IEnumerable<Reembolso>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<Reembolso> AdicionarAsync(Reembolso reembolso);
    Task<Reembolso> AtualizarAsync(Reembolso reembolso);
    Task<bool> ExcluirAsync(Guid id); // Alterado de Task para Task<bool>
} 