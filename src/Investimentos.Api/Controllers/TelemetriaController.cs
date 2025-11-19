using Investimentos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Investimentos.Api.Controllers;

[ApiController]
[Route("api")]
public class TelemetriaController : ControllerBase
{
    private readonly ITelemetriaService _telemetriaService;
    private readonly ILogger<TelemetriaController> _logger;

    public TelemetriaController(
        ITelemetriaService telemetriaService,
        ILogger<TelemetriaController> logger)
    {
        _telemetriaService = telemetriaService;
        _logger = logger;
    }

    /// <summary>
    /// Retorna dados de telemetria com volumes e tempos de resposta
    /// </summary>
    /// <param name="dataInicio">Data inicial do período (opcional, padrão: último mês)</param>
    /// <param name="dataFim">Data final do período (opcional, padrão: hoje)</param>
    /// <returns>Estatísticas de telemetria por serviço</returns>
    [HttpGet("telemetria")]
    public async Task<ActionResult> ObterTelemetria(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        _logger.LogInformation(
            "Requisição de telemetria - Período: {Inicio} a {Fim}",
            dataInicio?.ToString("yyyy-MM-dd") ?? "último mês",
            dataFim?.ToString("yyyy-MM-dd") ?? "hoje");

        try
        {
            var telemetria = await _telemetriaService.ObterTelemetriaAsync(dataInicio, dataFim);
            return Ok(telemetria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter telemetria");
            return StatusCode(500, new { error = "Erro ao processar dados de telemetria" });
        }
    }
}
