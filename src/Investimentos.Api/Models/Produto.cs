namespace Investimentos.Api.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // CDB, LCI, LCA, Tesouro Direto, Fundo
    public decimal Rentabilidade { get; set; }
    public string Risco { get; set; } = string.Empty; // Baixo, Médio, Alto
    public int PrazoMinimoDias { get; set; }
    public decimal ValorMinimoInvestimento { get; set; }
    public bool LiquidezImediata { get; set; }
    public string? PerfilRiscoRecomendado { get; set; } // Conservador, Moderado, Agressivo
    public DateTime DataCriacao { get; set; }
    
    // Relacionamentos
    public ICollection<Simulacao> Simulacoes { get; set; } = new List<Simulacao>();
}
