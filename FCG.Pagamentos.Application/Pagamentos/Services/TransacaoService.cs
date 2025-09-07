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
        // Valida dados específicos do tipo de pagamento
        ValidarDadosPagamento(request.TipoPagamento, request);

        var transacao = new Transacao
        {
            UsuarioId = request.UsuarioId,
            JogoId = request.JogoId,
            Valor = request.Valor,
            TipoPagamento = request.TipoPagamento,
            Status = StatusTransacao.Processando,
            Referencia = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper(),
            DataProcessamento = DateTime.UtcNow
        };

        // Processa o pagamento imediatamente
        var random = new Random();
        var sucesso = ProcessarTipoPagamento(request.TipoPagamento, random);

        if (sucesso)
        {
            transacao.Status = StatusTransacao.Aprovada;
            transacao.DataConfirmacao = DateTime.UtcNow;
            transacao.CodigoAutorizacao = GerarCodigoAutorizacao(request.TipoPagamento);
            transacao.CodigoTransacao = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
            transacao.Observacoes = GerarObservacoesPagamento(request.TipoPagamento, request);
        }
        else
        {
            transacao.Status = StatusTransacao.Recusada;
            transacao.ErroProcessamento = GerarErroProcessamento(request.TipoPagamento);
        }

        var transacaoCriada = await _transacaoRepository.AdicionarAsync(transacao);
        return MapearParaResponse(transacaoCriada);
    }

    public async Task<TransacaoResponse?> ObterPorIdAsync(Guid id)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(id);
        return transacao != null ? MapearParaResponse(transacao) : null;
    }

    public async Task<IEnumerable<TransacaoResponse>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        var transacoes = await _transacaoRepository.ObterPorUsuarioAsync(usuarioId);
        return transacoes.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<TransacaoResponse>> ObterPorJogoAsync(Guid jogoId)
    {
        var transacoes = await _transacaoRepository.ObterPorJogoAsync(jogoId);
        return transacoes.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<TransacaoResponse>> BuscarAsync(BuscarTransacoesRequest request)
    {
        IEnumerable<Transacao> transacoes;

        if (request.JogoId.HasValue)
            transacoes = await _transacaoRepository.ObterPorJogoAsync(request.JogoId.Value);
        else if (request.DataInicio.HasValue && request.DataFim.HasValue)
            transacoes = await _transacaoRepository.ObterPorPeriodoAsync(request.DataInicio.Value, request.DataFim.Value);
        else
            transacoes = await _transacaoRepository.ObterTodosAsync();

        return transacoes.Select(MapearParaResponse);
    }

    public async Task<TransacaoResponse> AtualizarAsync(Guid id, AtualizarTransacaoRequest request)
    {
        var transacao = await _transacaoRepository.ObterPorIdAsync(id);
        if (transacao == null)
            throw new InvalidOperationException("Transação não encontrada");

        if (request.Status.HasValue) transacao.Status = request.Status.Value;
        if (request.Observacoes != null) transacao.Observacoes = request.Observacoes;

        var transacaoAtualizada = await _transacaoRepository.AtualizarAsync(transacao);
        return MapearParaResponse(transacaoAtualizada);
    }

    private static void ValidarDadosPagamento(TipoPagamento tipoPagamento, CriarTransacaoRequest request)
    {
        switch (tipoPagamento)
        {
            case TipoPagamento.CartaoCredito:
            case TipoPagamento.CartaoDebito:
                if (request.DadosCartao == null)
                    throw new InvalidOperationException("Dados do cartão são obrigatórios para pagamento com cartão");
                
                if (string.IsNullOrWhiteSpace(request.DadosCartao.NumeroCartao))
                    throw new InvalidOperationException("Número do cartão é obrigatório");
                if (string.IsNullOrWhiteSpace(request.DadosCartao.NomeTitular))
                    throw new InvalidOperationException("Nome do titular é obrigatório");
                if (string.IsNullOrWhiteSpace(request.DadosCartao.DataValidade))
                    throw new InvalidOperationException("Data de validade é obrigatória");
                if (string.IsNullOrWhiteSpace(request.DadosCartao.CVV))
                    throw new InvalidOperationException("CVV é obrigatório");
                break;
                
            case TipoPagamento.PIX:
                if (request.DadosPIX == null)
                    throw new InvalidOperationException("Dados PIX são obrigatórios para pagamento PIX");
                
                if (string.IsNullOrWhiteSpace(request.DadosPIX.ChavePIX))
                    throw new InvalidOperationException("Chave PIX é obrigatória");
                break;
                
            case TipoPagamento.Boleto:
                if (request.DadosBoleto == null)
                    throw new InvalidOperationException("Dados do boleto são obrigatórios para pagamento com boleto");
                
                if (string.IsNullOrWhiteSpace(request.DadosBoleto.CpfCnpj))
                    throw new InvalidOperationException("CPF/CNPJ é obrigatório");
                if (string.IsNullOrWhiteSpace(request.DadosBoleto.NomePagador))
                    throw new InvalidOperationException("Nome do pagador é obrigatório");
                if (string.IsNullOrWhiteSpace(request.DadosBoleto.Endereco))
                    throw new InvalidOperationException("Endereço é obrigatório");
                if (string.IsNullOrWhiteSpace(request.DadosBoleto.CEP))
                    throw new InvalidOperationException("CEP é obrigatório");
                if (string.IsNullOrWhiteSpace(request.DadosBoleto.Cidade))
                    throw new InvalidOperationException("Cidade é obrigatória");
                if (string.IsNullOrWhiteSpace(request.DadosBoleto.Estado))
                    throw new InvalidOperationException("Estado é obrigatório");
                break;
        }
    }

    private static bool ProcessarTipoPagamento(TipoPagamento tipoPagamento, Random random)
    {
        return tipoPagamento switch
        {
            TipoPagamento.PIX => random.Next(1, 11) <= 9, // 90% sucesso PIX
            TipoPagamento.CartaoCredito => random.Next(1, 11) <= 8, // 80% sucesso cartão crédito
            TipoPagamento.CartaoDebito => random.Next(1, 11) <= 8, // 80% sucesso cartão débito
            TipoPagamento.Boleto => random.Next(1, 11) <= 7, // 70% sucesso boleto
            _ => false
        };
    }

    private static string GerarCodigoAutorizacao(TipoPagamento tipoPagamento)
    {
        return tipoPagamento switch
        {
            TipoPagamento.PIX => $"PIX{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
            TipoPagamento.CartaoCredito => $"CC{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
            TipoPagamento.CartaoDebito => $"CD{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
            TipoPagamento.Boleto => $"BOL{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
            _ => Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
        };
    }

    private static string GerarObservacoesPagamento(TipoPagamento tipoPagamento, CriarTransacaoRequest request)
    {
        return tipoPagamento switch
        {
            TipoPagamento.PIX => $"PIX processado - Chave: {request.DadosPIX?.ChavePIX}",
            TipoPagamento.CartaoCredito => $"Cartão de crédito - Parcelas: {request.DadosCartao?.Parcelas ?? 1}",
            TipoPagamento.CartaoDebito => "Cartão de débito processado",
            TipoPagamento.Boleto => $"Boleto gerado - CPF/CNPJ: {request.DadosBoleto?.CpfCnpj}",
            _ => "Pagamento processado"
        };
    }

    private static string GerarErroProcessamento(TipoPagamento tipoPagamento)
    {
        return tipoPagamento switch
        {
            TipoPagamento.PIX => "Erro no processamento PIX - Chave inválida ou indisponível",
            TipoPagamento.CartaoCredito => "Cartão de crédito recusado - Verifique os dados ou limite",
            TipoPagamento.CartaoDebito => "Cartão de débito recusado - Saldo insuficiente",
            TipoPagamento.Boleto => "Erro na geração do boleto - Dados inválidos",
            _ => "Erro no processamento do pagamento"
        };
    }

    private static TransacaoResponse MapearParaResponse(Transacao transacao)
    {
        return new TransacaoResponse
        {
            Id = transacao.Id,
            UsuarioId = transacao.UsuarioId,
            JogoId = transacao.JogoId,
            Valor = transacao.Valor,
            Status = transacao.Status,
            TipoPagamento = transacao.TipoPagamento,
            CodigoAutorizacao = transacao.CodigoAutorizacao,
            CodigoTransacao = transacao.CodigoTransacao,
            DataProcessamento = transacao.DataProcessamento,
            DataConfirmacao = transacao.DataConfirmacao,
            Observacoes = transacao.Observacoes,
            ErroProcessamento = transacao.ErroProcessamento,
            Referencia = transacao.Referencia,
            DataCriacao = transacao.DataCriacao
        };
    }
}