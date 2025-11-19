using Investimentos.Api.Models;

namespace Investimentos.Api.Services;

public class RecomendacaoService : IRecomendacaoService
{
    private readonly ILogger<RecomendacaoService> _logger;

    public RecomendacaoService(ILogger<RecomendacaoService> logger)
    {
        _logger = logger;
    }

    public (string perfil, int pontuacao) CalcularPerfil(Cliente cliente)
    {
        _logger.LogInformation("Calculando perfil de risco para cliente {ClienteId}", cliente.Id);

        // Algoritmo de cálculo de pontuação (0-100)
        int pontuacao = 0;

        // 1. Volume de investimentos (0-40 pontos)
        // Quanto maior o saldo, mais agressivo pode ser
        decimal saldoNormalizado = Math.Min(cliente.SaldoTotal / 100000m, 1m); // Normaliza até 100k
        int pontosVolume = (int)(saldoNormalizado * 40);
        pontuacao += pontosVolume;

        _logger.LogDebug("Pontos por volume (saldo {Saldo}): {Pontos}", cliente.SaldoTotal, pontosVolume);

        // 2. Frequência de movimentações (0-30 pontos)
        // Mais movimentações = mais propenso a risco
        int pontosFrequencia = Math.Min(cliente.FrequenciaMovimentacoes * 3, 30);
        pontuacao += pontosFrequencia;

        _logger.LogDebug("Pontos por frequência ({Frequencia} movimentações): {Pontos}", 
            cliente.FrequenciaMovimentacoes, pontosFrequencia);

        // 3. Preferência por liquidez ou rentabilidade (0-30 pontos)
        // Se prefere liquidez = conservador (menos pontos)
        // Se não prefere liquidez = busca rentabilidade (mais pontos)
        int pontosPreferencia = cliente.PrefereLiquidez ? 0 : 30;
        pontuacao += pontosPreferencia;

        _logger.LogDebug("Pontos por preferência (liquidez={Liquidez}): {Pontos}", 
            cliente.PrefereLiquidez, pontosPreferencia);

        // Garantir que a pontuação está entre 0 e 100
        pontuacao = Math.Clamp(pontuacao, 0, 100);

        // Mapear pontuação para perfil
        string perfil = pontuacao switch
        {
            <= 40 => "Conservador",
            <= 70 => "Moderado",
            _ => "Agressivo"
        };

        _logger.LogInformation(
            "Perfil calculado: {Perfil} (pontuação: {Pontuacao}) - " +
            "Saldo: {Saldo}, Frequência: {Frequencia}, Liquidez: {Liquidez}",
            perfil, pontuacao, cliente.SaldoTotal, cliente.FrequenciaMovimentacoes, cliente.PrefereLiquidez);

        return (perfil, pontuacao);
    }
}
