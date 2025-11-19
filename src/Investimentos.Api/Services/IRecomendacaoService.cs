using Investimentos.Api.Models;

namespace Investimentos.Api.Services;

public interface IRecomendacaoService
{
    (string perfil, int pontuacao) CalcularPerfil(Cliente cliente);
}
