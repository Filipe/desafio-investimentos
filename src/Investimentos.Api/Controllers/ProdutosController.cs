using Investimentos.Api.Data;
using Investimentos.Api.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Controllers;

[ApiController]
[Route("api")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(
        AppDbContext context,
        IMapper mapper,
        ILogger<ProdutosController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retorna produtos recomendados baseado no perfil de risco
    /// </summary>
    /// <param name="perfil">Nome do perfil: Conservador, Moderado ou Agressivo</param>
    /// <returns>Lista de produtos filtrados por risco e ordenados por rentabilidade</returns>
    [HttpGet("produtos-recomendados/{perfil}")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutosRecomendados(string perfil)
    {
        _logger.LogInformation("Buscando produtos recomendados para perfil: {Perfil}", perfil);

        // Normalizar o nome do perfil
        perfil = perfil.Trim();

        // Validar perfil
        var perfisValidos = new[] { "Conservador", "Moderado", "Agressivo" };
        if (!perfisValidos.Contains(perfil, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Perfil inválido solicitado: {Perfil}", perfil);
            return BadRequest(new { 
                error = "Perfil inválido. Use: Conservador, Moderado ou Agressivo" 
            });
        }

        // Definir mapeamento de perfil para riscos aceitáveis
        var riscosAceitaveis = perfil.ToLower() switch
        {
            "conservador" => new[] { "Baixo", "Médio" },
            "moderado" => new[] { "Baixo", "Médio", "Alto" }, // Moderado aceita todos
            "agressivo" => new[] { "Alto" },
            _ => Array.Empty<string>()
        };

        _logger.LogDebug("Riscos aceitáveis para {Perfil}: {Riscos}", 
            perfil, string.Join(", ", riscosAceitaveis));

        // Buscar produtos filtrados por risco
        var produtos = await _context.Produtos
            .Where(p => riscosAceitaveis.Contains(p.Risco))
            .ToListAsync();

        // Ordenar por rentabilidade em memória
        produtos = produtos
            .OrderByDescending(p => p.Rentabilidade)
            .ToList();

        _logger.LogInformation(
            "Encontrados {Count} produtos para perfil {Perfil}", 
            produtos.Count, perfil);

        var produtosDto = _mapper.Map<List<ProdutoDto>>(produtos);

        return Ok(produtosDto);
    }
}
