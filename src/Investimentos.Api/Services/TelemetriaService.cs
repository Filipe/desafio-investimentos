using Investimentos.Api.Data;
using Investimentos.Api.DTOs;
using Investimentos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Services;

public class TelemetriaService : ITelemetriaService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TelemetriaService> _logger;

    public TelemetriaService(AppDbContext context, ILogger<TelemetriaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RegistrarChamadaAsync(string nomeServico, string endpoint, string metodoHttp, long tempoRespostaMs, int statusCode)
    {
        try
        {
            var registro = new TelemetriaRegistro
            {
                NomeServico = nomeServico,
                Endpoint = endpoint,
                MetodoHttp = metodoHttp,
                TempoRespostaMs = tempoRespostaMs,
                StatusCode = statusCode,
                DataChamada = DateTime.UtcNow
            };

            _context.TelemetriaRegistros.Add(registro);
            await _context.SaveChangesAsync();

            _logger.LogDebug(
                "Telemetria registrada: {Servico} {Metodo} {Endpoint} - {Tempo}ms - Status {Status}",
                nomeServico, metodoHttp, endpoint, tempoRespostaMs, statusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar telemetria para {Servico}", nomeServico);
            // Não propaga a exceção para não afetar a resposta da API
        }
    }

    public async Task<TelemetriaDto> ObterTelemetriaAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        _logger.LogInformation("Obtendo dados de telemetria");

        // Define período padrão (último mês)
        var inicio = (dataInicio ?? DateTime.UtcNow.AddMonths(-1)).Date;
        var fim = (dataFim ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1); // Fim do dia

        // Busca registros no período
        var registros = await _context.TelemetriaRegistros
            .Where(t => t.DataChamada >= inicio && t.DataChamada <= fim)
            .ToListAsync();

        if (!registros.Any())
        {
            _logger.LogWarning("Nenhum registro de telemetria encontrado no período");
            return new TelemetriaDto
            {
                Servicos = new List<ServicoTelemetriaDto>(),
                Periodo = new PeriodoDto
                {
                    Inicio = inicio.ToString("yyyy-MM-dd"),
                    Fim = fim.ToString("yyyy-MM-dd")
                }
            };
        }

        // Agrupa por serviço e calcula estatísticas
        var servicosAgrupados = registros
            .GroupBy(r => r.NomeServico)
            .Select(g => new ServicoTelemetriaDto
            {
                Nome = g.Key,
                QuantidadeChamadas = g.Count(),
                MediaTempoRespostaMs = Math.Round(g.Average(r => r.TempoRespostaMs), 2)
            })
            .OrderByDescending(s => s.QuantidadeChamadas)
            .ToList();

        var primeiraData = registros.Min(r => r.DataChamada);
        var ultimaData = registros.Max(r => r.DataChamada);

        _logger.LogInformation(
            "Telemetria processada: {Servicos} serviços, {Total} chamadas totais",
            servicosAgrupados.Count, registros.Count);

        return new TelemetriaDto
        {
            Servicos = servicosAgrupados,
            Periodo = new PeriodoDto
            {
                Inicio = primeiraData.ToString("yyyy-MM-dd"),
                Fim = ultimaData.ToString("yyyy-MM-dd")
            }
        };
    }
}
