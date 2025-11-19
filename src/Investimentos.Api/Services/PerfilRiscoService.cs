using Investimentos.Api.Data;
using Investimentos.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Services;

public class PerfilRiscoService : IPerfilRiscoService
{
    private readonly AppDbContext _context;
    private readonly IRecomendacaoService _recomendacaoService;
    private readonly ILogger<PerfilRiscoService> _logger;

    public PerfilRiscoService(
        AppDbContext context,
        IRecomendacaoService recomendacaoService,
        ILogger<PerfilRiscoService> logger)
    {
        _context = context;
        _recomendacaoService = recomendacaoService;
        _logger = logger;
    }

    public async Task<PerfilRiscoDto?> ObterPerfilAsync(int clienteId)
    {
        _logger.LogInformation("Obtendo perfil de risco para cliente {ClienteId}", clienteId);

        var cliente = await _context.Clientes
            .Include(c => c.PerfilRisco)
            .FirstOrDefaultAsync(c => c.Id == clienteId);

        if (cliente == null)
        {
            _logger.LogWarning("Cliente {ClienteId} não encontrado", clienteId);
            return null;
        }

        // Calcular perfil dinâmico baseado no comportamento atual
        var (perfilCalculado, pontuacao) = _recomendacaoService.CalcularPerfil(cliente);

        // Buscar a descrição do perfil no banco
        var perfilRiscoDb = await _context.PerfisRisco
            .FirstOrDefaultAsync(p => p.Nome == perfilCalculado);

        var descricao = perfilRiscoDb?.Descricao ?? 
            "Perfil calculado dinamicamente baseado no comportamento do cliente";

        var resultado = new PerfilRiscoDto
        {
            ClienteId = clienteId,
            Nome = perfilCalculado,
            Pontuacao = pontuacao,
            Descricao = descricao
        };

        _logger.LogInformation(
            "Perfil obtido para cliente {ClienteId}: {Perfil} ({Pontuacao} pontos)",
            clienteId, perfilCalculado, pontuacao);

        return resultado;
    }
}
