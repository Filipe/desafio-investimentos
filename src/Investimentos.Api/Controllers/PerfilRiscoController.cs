using System.ComponentModel;
using Investimentos.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investimentos.Api.Controllers;

[ApiController]
[Route("api")]
public class PerfilRiscoController : ControllerBase
{
    private readonly IPerfilRiscoService _perfilRiscoService;
    private readonly ILogger<PerfilRiscoController> _logger;

    public PerfilRiscoController(
        IPerfilRiscoService perfilRiscoService,
        ILogger<PerfilRiscoController> logger)
    {
        _perfilRiscoService = perfilRiscoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o perfil de risco calculado dinamicamente para um cliente
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <returns>Perfil de risco com nome, pontuação e descrição</returns>
    [Authorize]
    [HttpGet("perfil-risco/{clienteId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ObterPerfilRisco([DefaultValue(1)] int clienteId)
    {
        _logger.LogInformation("Requisição para obter perfil do cliente {ClienteId}", clienteId);

        var perfil = await _perfilRiscoService.ObterPerfilAsync(clienteId);

        if (perfil == null)
        {
            _logger.LogWarning("Cliente {ClienteId} não encontrado", clienteId);
            return NotFound(new { error = $"Cliente com ID {clienteId} não encontrado" });
        }

        return Ok(perfil);
    }
}
