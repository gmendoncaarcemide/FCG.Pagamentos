namespace FCG.Pagamentos.Application.Messaging.Events;

public class PagamentoRecusadoEvent : IntegrationEvent
{
    public Guid TransacaoId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal Valor { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime DataRecusa { get; set; }
}
