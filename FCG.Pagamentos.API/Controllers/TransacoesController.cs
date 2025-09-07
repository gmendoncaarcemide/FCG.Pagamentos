using System.ComponentModel.DataAnnotations;
using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Pagamentos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransacoesController : ControllerBase
{
    private readonly ITransacaoService _service;
    
    public TransacoesController(ITransacaoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Criar e processar uma nova transação de pagamento
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransacaoResponse), 201)]
    [ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CriarTransacao([FromBody] CriarTransacaoRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            return BadRequest(validationResults);
        
        try
        {
            var transacao = await _service.CriarAsync(request);
            return Created($"/api/transacoes/{transacao.Id}", transacao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtém uma transação por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransacaoResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var transacao = await _service.ObterPorIdAsync(id);
            if (transacao == null)
                return NotFound($"Transação com ID {id} não encontrada");
            
            return Ok(transacao);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtém transações por usuário
    /// </summary>
    [HttpGet("usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TransacaoResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ObterPorUsuario(Guid usuarioId)
    {
        try
        {
            var transacoes = await _service.ObterPorUsuarioAsync(usuarioId);
            return Ok(transacoes);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtém transações por jogo
    /// </summary>
    [HttpGet("jogo/{jogoId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TransacaoResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ObterPorJogo(Guid jogoId)
    {
        try
        {
            var transacoes = await _service.ObterPorJogoAsync(jogoId);
            return Ok(transacoes);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Busca transações com filtros
    /// </summary>
    [HttpPost("buscar")]
    [ProducesResponseType(typeof(IEnumerable<TransacaoResponse>), 200)]
    [ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Buscar([FromBody] BuscarTransacoesRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            return BadRequest(validationResults);
        
        try
        {
            var transacoes = await _service.BuscarAsync(request);
            return Ok(transacoes);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Atualiza status e observações de uma transação
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TransacaoResponse), 200)]
    [ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTransacaoRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            return BadRequest(validationResults);
        
        try
        {
            var transacao = await _service.AtualizarAsync(id, request);
            return Ok(transacao);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("não encontrada"))
                return NotFound(ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }
}
