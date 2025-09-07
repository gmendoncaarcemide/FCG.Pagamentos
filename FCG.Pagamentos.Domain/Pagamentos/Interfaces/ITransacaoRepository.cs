using FCG.Pagamentos.Domain.Pagamentos.Entities;

namespace FCG.Pagamentos.Domain.Pagamentos.Interfaces;

public interface ITransacaoRepository
{
    Task<Transacao?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Transacao>> ObterTodosAsync();
    Task<IEnumerable<Transacao>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<Transacao>> ObterPorJogoAsync(Guid jogoId);
    Task<IEnumerable<Transacao>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<Transacao> AdicionarAsync(Transacao transacao);
    Task<Transacao> AtualizarAsync(Transacao transacao);
} 