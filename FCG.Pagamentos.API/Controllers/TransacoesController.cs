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

    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), 200)]
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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ObterTodos()
    {
        try
        {
            var transacoes = await _service.ObterTodosAsync();
            return Ok(transacoes);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), 200)]
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

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        try
        {
            await _service.ExcluirAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    [HttpPost("processar")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ProcessarPagamento([FromBody] ProcessarPagamentoRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            return BadRequest(validationResults);
        
        try
        {
            var transacao = await _service.ProcessarPagamentoAsync(request);
            return Ok(transacao);
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

    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CancelarTransacao(Guid id)
    {
        try
        {
            var transacao = await _service.CancelarTransacaoAsync(id);
            return Ok(transacao);
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

    [HttpPost("buscar")]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
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

    [HttpGet("referencia/{referencia}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ObterPorReferencia(string referencia)
    {
        try
        {
            var transacao = await _service.ObterPorReferenciaAsync(referencia);
            if (transacao == null)
                return NotFound($"Transação com referência {referencia} não encontrada");
            
            return Ok(transacao);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }
}
