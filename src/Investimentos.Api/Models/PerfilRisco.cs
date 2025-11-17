namespace Investimentos.Api.Models;

public class PerfilRisco
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty; // Conservador, Moderado, Agressivo
    public string Descricao { get; set; } = string.Empty;
    public int PontuacaoMinima { get; set; }
    public int PontuacaoMaxima { get; set; }
    
    // Relacionamentos
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}
