namespace FCG.Pagamentos.Application.Messaging.Events;

public class PagamentoAprovadoEvent : IntegrationEvent
{
    public Guid TransacaoId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal Valor { get; set; }
    public string CodigoAutorizacao { get; set; } = string.Empty;
    public DateTime DataAprovacao { get; set; }
}
