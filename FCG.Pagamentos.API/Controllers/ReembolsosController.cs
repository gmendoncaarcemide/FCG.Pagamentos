using System.ComponentModel.DataAnnotations;
using FCG.Pagamentos.Application.Pagamentos.Interfaces;
using FCG.Pagamentos.Application.Pagamentos.ViewModels;
using FCG.Pagamentos.Domain.Pagamentos.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Pagamentos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReembolsosController : ControllerBase
{
    private readonly IReembolsoService _service;
    
    public ReembolsosController(IReembolsoService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CriarReembolso([FromBody] CriarReembolsoRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            return BadRequest(validationResults);
        
        try
        {
            var reembolso = await _service.CriarAsync(request);
            return Created($"/api/reembolsos/{reembolso.Id}", reembolso);
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
            var reembolso = await _service.ObterPorIdAsync(id);
            if (reembolso == null)
                return NotFound($"Reembolso com ID {id} não encontrado");
            
            return Ok(reembolso);
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
            var reembolsos = await _service.ObterTodosAsync();
            return Ok(reembolsos);
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
            var reembolsos = await _service.ObterPorUsuarioAsync(usuarioId);
            return Ok(reembolsos);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    [HttpGet("transacao/{transacaoId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ObterPorTransacao(Guid transacaoId)
    {
        try
        {
            var reembolsos = await _service.ObterPorTransacaoAsync(transacaoId);
            return Ok(reembolsos);
        }
        catch (Exception ex)
        {
            return Problem($"Erro interno: {ex.Message}");
        }
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] string novoStatus)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(novoStatus))
                return BadRequest("Status é obrigatório");

            if (!Enum.TryParse<StatusReembolso>(novoStatus, true, out var statusEnum))
                return BadRequest("Status inválido");

            var reembolso = await _service.AtualizarStatusAsync(id, statusEnum);
            return Ok(reembolso);
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
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CancelarReembolso(Guid id)
    {
        try
        {
            await _service.CancelarReembolsoAsync(id);
            return NoContent();
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

    [HttpPost("{id:guid}/processar")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ProcessarReembolso(Guid id)
    {
        try
        {
            var reembolso = await _service.ProcessarReembolsoAsync(id);
            return Ok(reembolso);
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
}
