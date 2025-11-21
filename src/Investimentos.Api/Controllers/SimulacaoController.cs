using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Investimentos.Api.DTOs;
using Investimentos.Api.Services;

namespace Investimentos.Api.Controllers;

[ApiController]
[Route("api")]
public class SimulacaoController : ControllerBase
{
    private readonly ISimulacaoService _simulacaoService;
    private readonly ILogger<SimulacaoController> _logger;

    public SimulacaoController(ISimulacaoService simulacaoService, ILogger<SimulacaoController> logger)
    {
        _simulacaoService = simulacaoService;
        _logger = logger;
    }

    /// <summary>
    /// Simula um investimento com base nos parâmetros fornecidos
    /// </summary>
    /// <param name="request">Dados da simulação</param>
    /// <returns>Resultado da simulação</returns>
    [Authorize]
    [HttpPost("simular-investimento")]
    [ProducesResponseType(typeof(SimulacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult<SimulacaoResponse>> SimularInvestimento([FromBody] SimulacaoRequest request)
    {
        try
        {
            _logger.LogInformation("Recebendo solicitação de simulação para cliente {ClienteId}", request.ClienteId);
            
            var resultado = await _simulacaoService.SimularInvestimentoAsync(request);
            
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar simulação de investimento");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao processar simulação" });
        }
    }

    /// <summary>
    /// Retorna todas as simulações realizadas
    /// </summary>
    /// <returns>Lista de simulações</returns>
    [Authorize]
    [HttpGet("simulacoes")]
    [ProducesResponseType(typeof(IEnumerable<SimulacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<SimulacaoDto>>> ObterSimulacoes()
    {
        try
        {
            _logger.LogInformation("Obtendo todas as simulações");
            
            var simulacoes = await _simulacaoService.ObterTodasSimulacoesAsync();
            
            return Ok(simulacoes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter simulações");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao obter simulações" });
        }
    }

    /// <summary>
    /// Retorna valores simulados agrupados por produto e dia
    /// </summary>
    /// <returns>Lista de simulações agrupadas por produto e dia</returns>
    [HttpGet("simulacoes/por-produto-dia")]
    [ProducesResponseType(typeof(IEnumerable<SimulacaoPorProdutoDiaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SimulacaoPorProdutoDiaDto>>> ObterSimulacoesPorProdutoDia()
    {
        try
        {
            _logger.LogInformation("Obtendo simulações agrupadas por produto e dia");
            
            var resultado = await _simulacaoService.GetSimulacoesPorProdutoPorDiaAsync();
            
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter simulações por produto e dia");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao obter simulações por produto e dia" });
        }
    }

    /// <summary>
    /// Retorna histórico de investimentos (simulações) de um cliente específico
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <returns>Lista de investimentos do cliente</returns>
    [HttpGet("investimentos/{clienteId}")]
    [ProducesResponseType(typeof(IEnumerable<InvestimentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InvestimentoDto>>> ObterInvestimentosPorCliente(int clienteId)
    {
        try
        {
            _logger.LogInformation("Obtendo histórico de investimentos para cliente {ClienteId}", clienteId);
            
            var investimentos = await _simulacaoService.ObterInvestimentosPorClienteAsync(clienteId);
            
            return Ok(investimentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter investimentos do cliente {ClienteId}", clienteId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro ao obter investimentos" });
        }
    }
}
