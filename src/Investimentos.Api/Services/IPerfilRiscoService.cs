using Investimentos.Api.DTOs;

namespace Investimentos.Api.Services;

public interface IPerfilRiscoService
{
    Task<PerfilRiscoDto?> ObterPerfilAsync(int clienteId);
}
