namespace Investimentos.Api.DTOs;

public class TelemetriaDto
{
    public List<ServicoTelemetriaDto> Servicos { get; set; } = new();
    public PeriodoDto Periodo { get; set; } = new();
}

public class ServicoTelemetriaDto
{
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeChamadas { get; set; }
    public double MediaTempoRespostaMs { get; set; }
}

public class PeriodoDto
{
    public string Inicio { get; set; } = string.Empty;
    public string Fim { get; set; } = string.Empty;
}
