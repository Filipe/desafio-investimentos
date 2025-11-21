using System.ComponentModel;

namespace Investimentos.Api.DTOs;

public class SimulacaoRequest
{
    [DefaultValue(1)]
    public int ClienteId { get; set; }
    
    [DefaultValue(10000)]
    public decimal Valor { get; set; }
    
    [DefaultValue(12)]
    public int PrazoMeses { get; set; }
    
    [DefaultValue("CDB")]
    public string TipoProduto { get; set; } = string.Empty;
}
