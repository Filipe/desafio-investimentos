using Investimentos.Api.DTOs;

namespace Investimentos.Api.Services;

public interface ITelemetriaService
{
    Task RegistrarChamadaAsync(string nomeServico, string endpoint, string metodoHttp, long tempoRespostaMs, int statusCode);
    Task<TelemetriaDto> ObterTelemetriaAsync(DateTime? dataInicio = null, DateTime? dataFim = null);
}
