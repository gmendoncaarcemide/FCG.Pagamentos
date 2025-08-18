using System.ComponentModel.DataAnnotations;
using FCG.Pagamentos.Domain.Pagamentos.Entities;

namespace FCG.Pagamentos.Application.Pagamentos.ViewModels;

public class CriarTransacaoRequest
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

    public string Moeda { get; set; } = "BRL";
    public string? Observacoes { get; set; }
}

public class AtualizarTransacaoRequest
{
    public StatusTransacao? Status { get; set; }
    public string? CodigoAutorizacao { get; set; }
    public string? CodigoTransacao { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public string? Observacoes { get; set; }
    public string? ErroProcessamento { get; set; }
    public int? TentativasProcessamento { get; set; }
    public DateTime? ProximaTentativa { get; set; }
}

public class TransacaoResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = string.Empty;
    public StatusTransacao Status { get; set; }
    public TipoPagamento TipoPagamento { get; set; }
    public string? CodigoAutorizacao { get; set; }
    public string? CodigoTransacao { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public string? Observacoes { get; set; }
    public string? ErroProcessamento { get; set; }
    public int TentativasProcessamento { get; set; }
    public DateTime? ProximaTentativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class ProcessarPagamentoRequest
{
    [Required(ErrorMessage = "ID da transação é obrigatório")]
    public Guid TransacaoId { get; set; }

    [Required(ErrorMessage = "Dados do cartão são obrigatórios")]
    public DadosCartaoRequest? DadosCartao { get; set; }

    public string? Observacoes { get; set; }
}

public class DadosCartaoRequest
{
    [Required(ErrorMessage = "Número do cartão é obrigatório")]
    [StringLength(19, MinimumLength = 13, ErrorMessage = "Número do cartão deve ter entre 13 e 19 dígitos")]
    public string NumeroCartao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome do titular é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome do titular deve ter no máximo 100 caracteres")]
    public string NomeTitular { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de validade é obrigatória")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Data de validade deve estar no formato MM/YY")]
    public string DataValidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV é obrigatório")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV deve ter entre 3 e 4 dígitos")]
    public string CVV { get; set; } = string.Empty;
}

public class BuscarTransacoesRequest
{
    public Guid? UsuarioId { get; set; }
    public Guid? JogoId { get; set; }
    public StatusTransacao? Status { get; set; }
    public TipoPagamento? TipoPagamento { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public decimal? ValorMinimo { get; set; }
    public decimal? ValorMaximo { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public string? Ordenacao { get; set; }
    public bool OrdemDecrescente { get; set; } = true;
} 