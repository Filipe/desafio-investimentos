namespace Investimentos.Api.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public decimal SaldoTotal { get; set; }
    public int FrequenciaMovimentacoes { get; set; }
    public bool PrefereLiquidez { get; set; }
    
    // Relacionamentos
    public int? PerfilRiscoId { get; set; }
    public PerfilRisco? PerfilRisco { get; set; }
    public ICollection<Simulacao> Simulacoes { get; set; } = new List<Simulacao>();
}
