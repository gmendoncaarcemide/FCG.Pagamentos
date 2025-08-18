using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using FCG.Pagamentos.Domain.Pagamentos.Entities;
using FCG.Pagamentos.Domain.Pagamentos.Interfaces;

namespace FCG.Pagamentos.Application.Pagamentos.Services;

public class TransacaoService : ITransacaoService
{
    private readonly ITransacaoRepository _transacaoRepository;

    public TransacaoService(ITransacaoRepository transacaoRepository)
    {
        _transacaoRepository = transacaoRepository;
    }

    public async Task<TransacaoResponse> CriarAsync(CriarTransacaoRequest request)
    {
        var transacao = new Transacao
        {
            UsuarioId = request.UsuarioId,
            JogoId = request.JogoId,
            Valor = request.Valor,
            Moeda = request.Moeda,
            TipoPagamento = request.TipoPagamento,
            Status = StatusTransacao.Pendente,
            Referencia = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper(),
            DetalhesPagamento = request.Observacoes ?? "Transação criada"
        };

        var transacaoCriada = await _transacaoRepository.AdicionarAsync(transacao);
        return MapearParaResponse(transacaoCriada);
    }

    public async Task<TransacaoResponse?> ObterPorIdAsync(Guid id)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(id);
        return transacao != null ? MapearParaResponse(transacao) : null;
    }

    public async Task<IEnumerable<TransacaoResponse>> ObterTodosAsync()
    {
        var transacoes = await _transacaoRepository.ObterTodosAsync();
        return transacoes.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<TransacaoResponse>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        var transacoes = await _transacaoRepository.ObterPorUsuarioAsync(usuarioId);
        return transacoes.Select(MapearParaResponse);
    }

    public async Task<TransacaoResponse> AtualizarAsync(Guid id, AtualizarTransacaoRequest request)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(id);
        if (transacao == null)
            throw new InvalidOperationException("Transação não encontrada");

        if (request.Status.HasValue) transacao.Status = request.Status.Value;
        if (request.CodigoAutorizacao != null) transacao.CodigoAutorizacao = request.CodigoAutorizacao;
        if (request.CodigoTransacao != null) transacao.CodigoTransacao = request.CodigoTransacao;
        if (request.DataProcessamento.HasValue) transacao.DataProcessamento = request.DataProcessamento.Value;
        if (request.DataConfirmacao.HasValue) transacao.DataConfirmacao = request.DataConfirmacao.Value;
        if (request.Observacoes != null) transacao.Observacoes = request.Observacoes;
        if (request.ErroProcessamento != null) transacao.ErroProcessamento = request.ErroProcessamento;
        if (request.TentativasProcessamento.HasValue) transacao.TentativasProcessamento = request.TentativasProcessamento.Value;
        if (request.ProximaTentativa.HasValue) transacao.ProximaTentativa = request.ProximaTentativa.Value;

        var transacaoAtualizada = await _transacaoRepository.AtualizarAsync(transacao);
        return MapearParaResponse(transacaoAtualizada);
    }

    public async Task ExcluirAsync(Guid id)
    {
        await _transacaoRepository.ExcluirAsync(id);
    }

    public async Task<TransacaoResponse> ProcessarPagamentoAsync(ProcessarPagamentoRequest request)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(request.TransacaoId);
        if (transacao == null)
            throw new InvalidOperationException("Transação não encontrada");

        if (transacao.Status != StatusTransacao.Pendente)
            throw new InvalidOperationException("Transação não está pendente");

        transacao.Status = StatusTransacao.Processando;
        transacao.DataProcessamento = DateTime.UtcNow;
        transacao.TentativasProcessamento++;

  
        await Task.Delay(1000);

        transacao.Status = StatusTransacao.Aprovada;
        transacao.DataConfirmacao = DateTime.UtcNow;
        transacao.CodigoAutorizacao = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        transacao.CodigoTransacao = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

        var transacaoProcessada = await _transacaoRepository.AtualizarAsync(transacao);
        return MapearParaResponse(transacaoProcessada);
    }

    public async Task<TransacaoResponse> CancelarTransacaoAsync(Guid id)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(id);
        if (transacao == null)
            throw new InvalidOperationException("Transação não encontrada");

        if (transacao.Status == StatusTransacao.Cancelada)
            throw new InvalidOperationException("Transação já está cancelada");

        transacao.Status = StatusTransacao.Cancelada;
        transacao.Observacoes = "Transação cancelada pelo usuário";

        var transacaoCancelada = await _transacaoRepository.AtualizarAsync(transacao);
        return MapearParaResponse(transacaoCancelada);
    }

    public async Task<IEnumerable<TransacaoResponse>> BuscarAsync(BuscarTransacoesRequest request)
    {
        IEnumerable<Transacao> transacoes;

        if (request.UsuarioId.HasValue)
            transacoes = await _transacaoRepository.ObterPorUsuarioAsync(request.UsuarioId.Value);
        else if (request.JogoId.HasValue)
            transacoes = await _transacaoRepository.ObterPorJogoAsync(request.JogoId.Value);
        else if (request.Status.HasValue)
            transacoes = await _transacaoRepository.ObterPorStatusAsync(request.Status.Value);
        else if (request.TipoPagamento.HasValue)
            transacoes = await _transacaoRepository.ObterPorTipoPagamentoAsync(request.TipoPagamento.Value);
        else if (request.DataInicio.HasValue && request.DataFim.HasValue)
            transacoes = await _transacaoRepository.ObterPorPeriodoAsync(request.DataInicio.Value, request.DataFim.Value);
        else
            transacoes = await _transacaoRepository.ObterTodosAsync();

        return transacoes.Select(MapearParaResponse);
    }

    public async Task<TransacaoResponse?> ObterPorReferenciaAsync(string referencia)
    {
        var transacao = await _transacaoRepository.ObterPorReferenciaAsync(referencia);
        return transacao != null ? MapearParaResponse(transacao) : null;
    }

    private static TransacaoResponse MapearParaResponse(Transacao transacao)
    {
        return new TransacaoResponse
        {
            Id = transacao.Id,
            UsuarioId = transacao.UsuarioId,
            JogoId = transacao.JogoId,
            Valor = transacao.Valor,
            Moeda = transacao.Moeda,
            Status = transacao.Status,
            TipoPagamento = transacao.TipoPagamento,
            CodigoAutorizacao = transacao.CodigoAutorizacao,
            CodigoTransacao = transacao.CodigoTransacao,
            DataProcessamento = transacao.DataProcessamento,
            DataConfirmacao = transacao.DataConfirmacao,
            Observacoes = transacao.Observacoes,
            ErroProcessamento = transacao.ErroProcessamento,
            TentativasProcessamento = transacao.TentativasProcessamento,
            ProximaTentativa = transacao.ProximaTentativa,
            DataCriacao = transacao.DataCriacao,
            DataAtualizacao = transacao.DataAtualizacao
        };
    }
} 