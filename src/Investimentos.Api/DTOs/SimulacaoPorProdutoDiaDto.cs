namespace Investimentos.Api.DTOs;

public class SimulacaoPorProdutoDiaDto
{
    public string Produto { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty; // formato yyyy-MM-dd
    public int QuantidadeSimulacoes { get; set; }
    public decimal MediaValorFinal { get; set; }
}
