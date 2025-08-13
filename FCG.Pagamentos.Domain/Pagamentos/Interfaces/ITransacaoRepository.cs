using FCG.Pagamentos.Domain.Pagamentos.Entities;

namespace FCG.Pagamentos.Domain.Pagamentos.Interfaces;

public interface ITransacaoRepository
{
    Task<Transacao?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Transacao>> ObterTodosAsync();
    Task<Transacao?> ObterPorReferenciaAsync(string referencia);
    Task<IEnumerable<Transacao>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<Transacao>> ObterPorJogoAsync(Guid jogoId);
    Task<IEnumerable<Transacao>> ObterPorStatusAsync(StatusTransacao status);
    Task<IEnumerable<Transacao>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<Transacao>> ObterPorTipoPagamentoAsync(TipoPagamento tipoPagamento);
    Task<Transacao> AdicionarAsync(Transacao transacao);
    Task<Transacao> AtualizarAsync(Transacao transacao);
    Task<bool> ExcluirAsync(Guid id); // Alterado de Task para Task<bool>
} 