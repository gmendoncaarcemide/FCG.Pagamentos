using System.ComponentModel.DataAnnotations;
using FCG.Pagamentos.Domain.Pagamentos.Entities;

namespace FCG.Pagamentos.Application.Pagamentos.ViewModels;

public class CriarReembolsoRequest
{
    [Required(ErrorMessage = "ID da transação é obrigatório")]
    public Guid TransacaoId { get; set; }

    [Required(ErrorMessage = "ID do usuário é obrigatório")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "Valor do reembolso é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorReembolso { get; set; }

    [Required(ErrorMessage = "Motivo é obrigatório")]
    [StringLength(500, ErrorMessage = "Motivo deve ter no máximo 500 caracteres")]
    public string Motivo { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Observações deve ter no máximo 1000 caracteres")]
    public string? Observacoes { get; set; }
}

public class ReembolsoResponse
{
    public Guid Id { get; set; }
    public Guid TransacaoId { get; set; }
    public Guid UsuarioId { get; set; }
    public decimal ValorReembolso { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string StatusReembolso { get; set; } = string.Empty;
    public DateTime DataSolicitacao { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public DateTime? DataConclusao { get; set; }
    public string? Observacoes { get; set; }
    public string? CodigoReembolso { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
} 