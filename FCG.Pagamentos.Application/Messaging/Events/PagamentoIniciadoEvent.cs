namespace FCG.Pagamentos.Application.Messaging.Events;

public class PagamentoIniciadoEvent : IntegrationEvent
{
    public Guid TransacaoId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal Valor { get; set; }
    public string TipoPagamento { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
}
