using AutoMapper;
using Investimentos.Api.DTOs;
using Investimentos.Api.Models;

namespace Investimentos.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeamento de Produto para DTOs
        CreateMap<Produto, ProdutoDto>();
        CreateMap<Produto, ProdutoValidadoDto>();
        CreateMap<ProdutoDto, Produto>();

        // Mapeamento de Simulacao para DTOs
        CreateMap<Simulacao, SimulacaoDto>()
            .ForMember(dest => dest.Produto, opt => opt.MapFrom(src => src.Produto.Nome));

        CreateMap<SimulacaoRequest, Simulacao>()
            .ForMember(dest => dest.ValorInvestido, opt => opt.MapFrom(src => src.Valor))
            .ForMember(dest => dest.DataSimulacao, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Cliente, opt => opt.Ignore())
            .ForMember(dest => dest.Produto, opt => opt.Ignore())
            .ForMember(dest => dest.ProdutoId, opt => opt.Ignore())
            .ForMember(dest => dest.ValorFinal, opt => opt.Ignore())
            .ForMember(dest => dest.RentabilidadeEfetiva, opt => opt.Ignore())
            .ForMember(dest => dest.TempoRespostaMs, opt => opt.Ignore());

        // Mapeamento de Cliente e Perfil de Risco
        CreateMap<Cliente, PerfilRiscoDto>()
            .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.PerfilRisco != null ? src.PerfilRisco.Nome : "Não definido"))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.PerfilRisco != null ? src.PerfilRisco.Descricao : ""))
            .ForMember(dest => dest.Pontuacao, opt => opt.Ignore());
    }
}
