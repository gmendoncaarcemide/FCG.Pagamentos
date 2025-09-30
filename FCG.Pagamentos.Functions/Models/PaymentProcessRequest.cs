using System.ComponentModel.DataAnnotations;
using FCG.Pagamentos.Domain.Pagamentos.Entities;

namespace FCG.Pagamentos.Functions.Models;

/// <summary>
/// Modelo de requisição para processamento de pagamento via Azure Function
/// </summary>
public class PaymentProcessRequest
{
    [Required(ErrorMessage = "ID do usuário é obrigatório")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "ID do jogo é obrigatório")]
    public Guid JogoId { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "Tipo de pagamento é obrigatório")]
    public TipoPagamento TipoPagamento { get; set; }

    // Dados específicos para cada tipo de pagamento (simplificado)
    public string? DadosPagamento { get; set; } // JSON serializado com os dados específicos

    public string? Observacoes { get; set; }
}

/// <summary>
/// Modelo de resposta para processamento de pagamento
/// </summary>
public class PaymentProcessResponse
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? TransacaoId { get; set; }
    public string? CodigoAutorizacao { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo para mensagens da fila de notificação
/// </summary>
public class NotificationMessage
{
    public Guid TransacaoId { get; set; }
    public Guid UsuarioId { get; set; }
    public string TipoNotificacao { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? DadosAdicionais { get; set; }
}
