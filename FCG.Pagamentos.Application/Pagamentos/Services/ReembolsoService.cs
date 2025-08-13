using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using FCG.Pagamentos.Domain.Pagamentos.Entities;
using FCG.Pagamentos.Domain.Pagamentos.Interfaces;

namespace FCG.Pagamentos.Application.Pagamentos.Services;

public class ReembolsoService : IReembolsoService
{
    private readonly IReembolsoRepository _reembolsoRepository;
    private readonly ITransacaoRepository _transacaoRepository;

    public ReembolsoService(IReembolsoRepository reembolsoRepository, ITransacaoRepository transacaoRepository)
    {
        _reembolsoRepository = reembolsoRepository;
        _transacaoRepository = transacaoRepository;
    }

    public async Task<ReembolsoResponse> CriarAsync(CriarReembolsoRequest request)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(request.TransacaoId);
        if (transacao == null)
            throw new InvalidOperationException("Transação não encontrada");

        if (transacao.Status != StatusTransacao.Aprovada)
            throw new InvalidOperationException("Apenas transações aprovadas podem ser reembolsadas");

        var reembolso = new Reembolso
        {
            TransacaoId = request.TransacaoId,
            UsuarioId = request.UsuarioId,
            ValorReembolso = request.ValorReembolso,
            Motivo = request.Motivo,
            Status = StatusReembolso.Solicitado,
            DataSolicitacao = DateTime.UtcNow
        };

        var reembolsoCriado = await _reembolsoRepository.AdicionarAsync(reembolso);
        return MapearParaResponse(reembolsoCriado);
    }

    public async Task<ReembolsoResponse?> ObterPorIdAsync(Guid id)
    {
        var reembolso = await _reembolsoRepository.ObterPorIdAsync(id);
        return reembolso != null ? MapearParaResponse(reembolso) : null;
    }

    public async Task<IEnumerable<ReembolsoResponse>> ObterTodosAsync()
    {
        var reembolsos = await _reembolsoRepository.ObterTodosAsync();
        return reembolsos.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<ReembolsoResponse>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        var reembolsos = await _reembolsoRepository.ObterPorUsuarioAsync(usuarioId);
        return reembolsos.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<ReembolsoResponse>> ObterPorTransacaoAsync(Guid transacaoId)
    {
        var reembolsos = await _reembolsoRepository.ObterPorTransacaoAsync(transacaoId);
        return reembolsos.Select(MapearParaResponse);
    }

    public async Task<ReembolsoResponse> AtualizarStatusAsync(Guid id, StatusReembolso status)
    {
        var reembolso = await _reembolsoRepository.ObterPorIdAsync(id);
        if (reembolso == null)
            throw new InvalidOperationException("Reembolso não encontrado");

        reembolso.Status = status;
        reembolso.DataProcessamento = DateTime.UtcNow;
        
        var reembolsoAtualizado = await _reembolsoRepository.AtualizarAsync(reembolso);
        return MapearParaResponse(reembolsoAtualizado);
    }

    public async Task CancelarReembolsoAsync(Guid id)
    {
        var reembolso = await _reembolsoRepository.ObterPorIdAsync(id);
        if (reembolso == null)
            throw new InvalidOperationException("Reembolso não encontrado");

        if (reembolso.Status != StatusReembolso.Solicitado)
            throw new InvalidOperationException("Apenas reembolsos solicitados podem ser cancelados");

        reembolso.Status = StatusReembolso.Recusado;
        reembolso.DataProcessamento = DateTime.UtcNow;
        
        await _reembolsoRepository.AtualizarAsync(reembolso);
    }

    public async Task<ReembolsoResponse> ProcessarReembolsoAsync(Guid id)
    {
        var reembolso = await _reembolsoRepository.ObterPorIdAsync(id);
        if (reembolso == null)
            throw new InvalidOperationException("Reembolso não encontrado");

        if (reembolso.Status != StatusReembolso.Solicitado)
            throw new InvalidOperationException("Apenas reembolsos solicitados podem ser processados");

        // Simular processamento do reembolso
        var random = new Random();
        var sucesso = random.Next(1, 11) <= 8; // 80% de chance de sucesso

        if (sucesso)
        {
            reembolso.Status = StatusReembolso.Aprovado;
            reembolso.DataProcessamento = DateTime.UtcNow;
        }
        else
        {
            reembolso.Status = StatusReembolso.Recusado;
            reembolso.DataProcessamento = DateTime.UtcNow;
        }

        var reembolsoProcessado = await _reembolsoRepository.AtualizarAsync(reembolso);
        return MapearParaResponse(reembolsoProcessado);
    }

    public async Task<IEnumerable<ReembolsoResponse>> ObterPorStatusAsync(StatusReembolso status)
    {
        var reembolsos = await _reembolsoRepository.ObterTodosAsync();
        return reembolsos.Where(r => r.Status == status).Select(MapearParaResponse);
    }

    public async Task<IEnumerable<ReembolsoResponse>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var reembolsos = await _reembolsoRepository.ObterTodosAsync();
        return reembolsos.Where(r => r.DataSolicitacao >= dataInicio && r.DataSolicitacao <= dataFim)
                        .Select(MapearParaResponse);
    }

    private static ReembolsoResponse MapearParaResponse(Reembolso reembolso)
    {
        return new ReembolsoResponse
        {
            Id = reembolso.Id,
            TransacaoId = reembolso.TransacaoId,
            UsuarioId = reembolso.UsuarioId,
            ValorReembolso = reembolso.ValorReembolso,
            Motivo = reembolso.Motivo,
            StatusReembolso = reembolso.Status.ToString(),
            DataSolicitacao = reembolso.DataSolicitacao,
            DataProcessamento = reembolso.DataProcessamento,
            DataCriacao = reembolso.DataCriacao,
            DataAtualizacao = reembolso.DataAtualizacao
        };
    }
} 