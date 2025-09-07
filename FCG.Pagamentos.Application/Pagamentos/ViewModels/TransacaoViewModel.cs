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

    // Dados específicos para cada tipo de pagamento
    public DadosCartaoRequest? DadosCartao { get; set; }
    public DadosPIXRequest? DadosPIX { get; set; }
    public DadosBoletoRequest? DadosBoleto { get; set; }

    public string? Observacoes { get; set; }
}

public class TransacaoResponse
{
    public Guid Id { get; set; }
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
    public DateTime DataCriacao { get; set; }
}

public class AtualizarTransacaoRequest
{
    public StatusTransacao? Status { get; set; }
    public string? Observacoes { get; set; }
}

public class BuscarTransacoesRequest
{
    public Guid? JogoId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}

public class DadosCartaoRequest
{
    [StringLength(19, MinimumLength = 13, ErrorMessage = "Número do cartão deve ter entre 13 e 19 dígitos")]
    public string NumeroCartao { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Nome do titular deve ter no máximo 100 caracteres")]
    public string NomeTitular { get; set; } = string.Empty;

    [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Data de validade deve estar no formato MM/YY")]
    public string DataValidade { get; set; } = string.Empty;

    [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV deve ter entre 3 e 4 dígitos")]
    public string CVV { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Parcelas deve estar entre 1 e 12")]
    public int? Parcelas { get; set; } = 1;
}

public class DadosPIXRequest
{
    [StringLength(100, ErrorMessage = "Chave PIX deve ter no máximo 100 caracteres")]
    public string ChavePIX { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Nome do beneficiário deve ter no máximo 100 caracteres")]
    public string? NomeBeneficiario { get; set; }
}

public class DadosBoletoRequest
{
    [StringLength(18, ErrorMessage = "CPF/CNPJ deve ter no máximo 18 caracteres")]
    public string CpfCnpj { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Nome do pagador deve ter no máximo 100 caracteres")]
    public string NomePagador { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Endereço deve ter no máximo 200 caracteres")]
    public string Endereco { get; set; } = string.Empty;

    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
    public string CEP { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Cidade deve ter no máximo 100 caracteres")]
    public string Cidade { get; set; } = string.Empty;

    [StringLength(2, MinimumLength = 2, ErrorMessage = "Estado deve ter 2 caracteres")]
    public string Estado { get; set; } = string.Empty;
}