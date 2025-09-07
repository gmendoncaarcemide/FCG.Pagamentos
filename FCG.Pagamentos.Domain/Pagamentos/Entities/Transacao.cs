using FCG.Pagamentos.Domain.Base;

namespace FCG.Pagamentos.Domain.Pagamentos.Entities;

public class Transacao : Entity
{
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal Valor { get; set; }
    public StatusTransacao Status { get; set; }
    public TipoPagamento TipoPagamento { get; set; }
    public string? CodigoAutorizacao { get; set; }
    public string? CodigoTransacao { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public string? Observacoes { get; set; }
    public string? ErroProcessamento { get; set; }
    public string Referencia { get; set; } = string.Empty;
}

public enum StatusTransacao
{
    Pendente = 1,
    Processando = 2,
    Aprovada = 3,
    Recusada = 4,
    Cancelada = 5,
    Falha = 6
}

public enum TipoPagamento
{
    CartaoCredito = 1,
    CartaoDebito = 2,
    PIX = 3,
    Boleto = 4
} 