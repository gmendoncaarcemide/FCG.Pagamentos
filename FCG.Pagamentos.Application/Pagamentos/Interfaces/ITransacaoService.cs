using FCG.Pagamentos.Application.Pagamentos.ViewModels;

namespace FCG.Pagamentos.Application.Pagamentos.Interfaces;

public interface ITransacaoService
{
    Task<TransacaoResponse> CriarAsync(CriarTransacaoRequest request);
    Task<TransacaoResponse?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<TransacaoResponse>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<TransacaoResponse>> ObterPorJogoAsync(Guid jogoId);
    Task<IEnumerable<TransacaoResponse>> BuscarAsync(BuscarTransacoesRequest request);
    Task<TransacaoResponse> AtualizarAsync(Guid id, AtualizarTransacaoRequest request);
} 