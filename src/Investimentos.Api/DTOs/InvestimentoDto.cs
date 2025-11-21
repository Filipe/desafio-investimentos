namespace Investimentos.Api.DTOs;

public class InvestimentoDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Rentabilidade { get; set; }
    public string Data { get; set; } = string.Empty;
}
