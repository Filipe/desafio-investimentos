using Investimentos.Api.DTOs;

namespace Investimentos.Api.Services;

public interface ISimulacaoService
{
    Task<SimulacaoResponse> SimularInvestimentoAsync(SimulacaoRequest request);
    Task<IEnumerable<SimulacaoDto>> ObterTodasSimulacoesAsync();
    Task<IEnumerable<SimulacaoPorProdutoDiaDto>> GetSimulacoesPorProdutoPorDiaAsync();
}
