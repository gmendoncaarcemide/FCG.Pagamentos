using FCG.Pagamentos.Domain.Base;

namespace FCG.Pagamentos.Domain.Pagamentos.Entities;

public class Reembolso : Entity
{
    public Guid TransacaoId { get; set; }
    public Guid UsuarioId { get; set; }
    public decimal ValorReembolso { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public StatusReembolso Status { get; set; }
    public DateTime DataSolicitacao { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public DateTime? DataConclusao { get; set; }
    public string? Observacoes { get; set; }
    public string? CodigoReembolso { get; set; }
    public virtual Transacao Transacao { get; set; } = null!;
}

public enum StatusReembolso
{
    Solicitado = 1,
    EmAnalise = 2,
    Aprovado = 3,
    Recusado = 4,
    Processando = 5,
    Concluido = 6
} 