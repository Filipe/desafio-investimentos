namespace Investimentos.Api.DTOs;

public class SimulacaoResponse
{
    public ProdutoValidadoDto ProdutoValidado { get; set; } = null!;
    public ResultadoSimulacaoDto ResultadoSimulacao { get; set; } = null!;
    public DateTime DataSimulacao { get; set; }
}

public class ProdutoValidadoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Rentabilidade { get; set; }
    public string Risco { get; set; } = string.Empty;
}

public class ResultadoSimulacaoDto
{
    public decimal ValorFinal { get; set; }
    public decimal RentabilidadeEfetiva { get; set; }
    public int PrazoMeses { get; set; }
}
