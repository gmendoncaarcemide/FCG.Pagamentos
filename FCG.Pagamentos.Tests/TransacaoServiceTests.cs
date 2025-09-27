using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCG.Pagamentos.Application.Pagamentos.Services;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using FCG.Pagamentos.Domain.Pagamentos.Entities;
using FCG.Pagamentos.Domain.Pagamentos.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace FCG.Pagamentos.Tests
{
    public class TransacaoServiceTests
    {
        private readonly Mock<ITransacaoRepository> _repoMock;
        private readonly TransacaoService _service;

        public TransacaoServiceTests()
        {
            _repoMock = new Mock<ITransacaoRepository>();
            _service = new TransacaoService(_repoMock.Object);
        }

        [Fact]
        public async Task CriarAsync_DeveCriarTransacaoComCartaoCredito()
        {
            var request = new CriarTransacaoRequest
            {
                UsuarioId = Guid.NewGuid(),
                JogoId = Guid.NewGuid(),
                Valor = 100,
                TipoPagamento = TipoPagamento.CartaoCredito,
                DadosCartao = new DadosCartaoRequest
                {
                    NumeroCartao = "1234567890123456",
                    NomeTitular = "Teste",
                    DataValidade = "12/30",
                    CVV = "123",
                    Parcelas = 1
                }
            };
            _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Transacao>()))
                .ReturnsAsync((Transacao t) => { t.Id = Guid.NewGuid(); t.DataCriacao = DateTime.UtcNow; return t; });

            var result = await _service.CriarAsync(request);

            result.Should().NotBeNull();
            result.UsuarioId.Should().Be(request.UsuarioId);
            result.JogoId.Should().Be(request.JogoId);
            result.Valor.Should().Be(request.Valor);
            result.TipoPagamento.Should().Be(request.TipoPagamento);
            result.Status.Should().BeOneOf(StatusTransacao.Aprovada, StatusTransacao.Recusada);
        }

        [Fact]
        public async Task CriarAsync_DeveLancarExcecao_QuandoDadosCartaoInvalidos()
        {
            var request = new CriarTransacaoRequest
            {
                UsuarioId = Guid.NewGuid(),
                JogoId = Guid.NewGuid(),
                Valor = 100,
                TipoPagamento = TipoPagamento.CartaoCredito,
                DadosCartao = null // obrigatório
            };
            Func<Task> act = async () => await _service.CriarAsync(request);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Dados do cartão são obrigatórios para pagamento com cartão");
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarTransacao_QuandoEncontrada()
        {
            var id = Guid.NewGuid();
            var transacao = new Transacao { Id = id, UsuarioId = Guid.NewGuid(), JogoId = Guid.NewGuid(), Valor = 10, Status = StatusTransacao.Aprovada, TipoPagamento = TipoPagamento.PIX, Referencia = "REF", DataCriacao = DateTime.UtcNow };
            _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(transacao);

            var result = await _service.ObterPorIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrada()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Transacao?)null);
            var result = await _service.ObterPorIdAsync(id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AtualizarAsync_DeveAtualizarTransacao_QuandoEncontrada()
        {
            var id = Guid.NewGuid();
            var transacao = new Transacao { Id = id, UsuarioId = Guid.NewGuid(), JogoId = Guid.NewGuid(), Valor = 10, Status = StatusTransacao.Processando, TipoPagamento = TipoPagamento.PIX, Referencia = "REF", DataCriacao = DateTime.UtcNow };
            _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(transacao);
            _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Transacao>())).ReturnsAsync((Transacao t) => t);
            var request = new AtualizarTransacaoRequest { Status = StatusTransacao.Aprovada, Observacoes = "ok" };

            var result = await _service.AtualizarAsync(id, request);

            result.Status.Should().Be(StatusTransacao.Aprovada);
            result.Observacoes.Should().Be("ok");
        }

        [Fact]
        public async Task AtualizarAsync_DeveLancarExcecao_QuandoTransacaoNaoEncontrada()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Transacao?)null);
            var request = new AtualizarTransacaoRequest { Status = StatusTransacao.Aprovada };
            Func<Task> act = async () => await _service.AtualizarAsync(id, request);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Transação não encontrada");
        }

        [Fact]
        public async Task ObterPorUsuarioAsync_DeveRetornarTransacoes()
        {
            var usuarioId = Guid.NewGuid();
            var transacoes = new List<Transacao> { new Transacao { Id = Guid.NewGuid(), UsuarioId = usuarioId, JogoId = Guid.NewGuid(), Valor = 10, Status = StatusTransacao.Aprovada, TipoPagamento = TipoPagamento.PIX, Referencia = "REF", DataCriacao = DateTime.UtcNow } };
            _repoMock.Setup(r => r.ObterPorUsuarioAsync(usuarioId)).ReturnsAsync(transacoes);
            var result = await _service.ObterPorUsuarioAsync(usuarioId);
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObterPorJogoAsync_DeveRetornarTransacoes()
        {
            var jogoId = Guid.NewGuid();
            var transacoes = new List<Transacao> { new Transacao { Id = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), JogoId = jogoId, Valor = 10, Status = StatusTransacao.Aprovada, TipoPagamento = TipoPagamento.PIX, Referencia = "REF", DataCriacao = DateTime.UtcNow } };
            _repoMock.Setup(r => r.ObterPorJogoAsync(jogoId)).ReturnsAsync(transacoes);
            var result = await _service.ObterPorJogoAsync(jogoId);
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }
    }
}