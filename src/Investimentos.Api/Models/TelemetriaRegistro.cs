namespace Investimentos.Api.Models;

public class TelemetriaRegistro
{
    public int Id { get; set; }
    public string NomeServico { get; set; } = string.Empty;
    public long TempoRespostaMs { get; set; }
    public DateTime DataChamada { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string MetodoHttp { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}
