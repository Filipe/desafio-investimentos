using Investimentos.Api.DTOs;
using Investimentos.Api.Data;
using Investimentos.Api.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Investimentos.Api.Services;

public class SimulacaoService : ISimulacaoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SimulacaoService> _logger;

    public SimulacaoService(AppDbContext context, IMapper mapper, ILogger<SimulacaoService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SimulacaoResponse> SimularInvestimentoAsync(SimulacaoRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // 1. Buscar produtos elegíveis no banco de dados
            var query = _context.Produtos.AsQueryable();

            // Filtro por valor mínimo
            query = query.Where(p => p.ValorMinimoInvestimento <= request.Valor);

            // Filtro por prazo mínimo (convertendo dias para meses: prazoMinimoDias/30)
            var prazoMinimoDias = request.PrazoMeses * 30;
            query = query.Where(p => p.PrazoMinimoDias <= prazoMinimoDias);

            // Filtro por tipo de produto (se informado)
            if (!string.IsNullOrWhiteSpace(request.TipoProduto))
            {
                query = query.Where(p => p.Tipo == request.TipoProduto);
            }

            var produtosElegiveis = await query.ToListAsync();

            // 2. Validar se existe produto elegível
            if (!produtosElegiveis.Any())
            {
                throw new InvalidOperationException(
                    $"Não foi encontrado nenhum produto elegível para os parâmetros informados. " +
                    $"Valor: {request.Valor:C}, Prazo: {request.PrazoMeses} meses, Tipo: {request.TipoProduto ?? "Qualquer"}");
            }

            // 3. Selecionar produto com menor risco (Baixo < Médio < Alto)
            var riscoOrdem = new Dictionary<string, int>
            {
                { "Baixo", 1 },
                { "Médio", 2 },
                { "Alto", 3 }
            };

            var produtoSelecionado = produtosElegiveis
                .OrderBy(p => riscoOrdem.ContainsKey(p.Risco) ? riscoOrdem[p.Risco] : int.MaxValue)
                .ThenByDescending(p => p.Rentabilidade)
                .First();

            _logger.LogInformation(
                "Produto selecionado: {ProdutoNome} (Risco: {Risco}, Rentabilidade: {Rentabilidade})",
                produtoSelecionado.Nome, produtoSelecionado.Risco, produtoSelecionado.Rentabilidade);

            // 4. Calcular valor final usando juros compostos
            // Fórmula: VF = VP * (1 + i)^n
            // onde: VP = valor presente, i = rentabilidade anual, n = prazo em anos
            var anos = request.PrazoMeses / 12.0;
            var valorFinal = request.Valor * (decimal)Math.Pow((double)(1 + produtoSelecionado.Rentabilidade), anos);
            var rentabilidadeEfetiva = (valorFinal - request.Valor) / request.Valor;

            _logger.LogInformation(
                "Simulação calculada: ValorInicial={ValorInicial:C}, ValorFinal={ValorFinal:C}, Rentabilidade={Rentabilidade:P}",
                request.Valor, valorFinal, rentabilidadeEfetiva);

            // 5. Criar e salvar Simulacao entity
            var simulacao = new Simulacao
            {
                ClienteId = request.ClienteId,
                ProdutoId = produtoSelecionado.Id,
                ValorInvestido = request.Valor,
                ValorFinal = valorFinal,
                RentabilidadeEfetiva = rentabilidadeEfetiva,
                PrazoMeses = request.PrazoMeses,
                DataSimulacao = DateTime.UtcNow,
                TempoRespostaMs = 0 // Será atualizado abaixo
            };

            _context.Simulacoes.Add(simulacao);
            await _context.SaveChangesAsync();

            stopwatch.Stop();
            
            // Atualizar tempo de resposta
            simulacao.TempoRespostaMs = stopwatch.ElapsedMilliseconds;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Simulação salva com sucesso. ID: {SimulacaoId}, Tempo: {Tempo}ms",
                simulacao.Id, simulacao.TempoRespostaMs);

            // 6. Retornar SimulacaoResponse preenchida
            return new SimulacaoResponse
            {
                ProdutoValidado = new ProdutoValidadoDto
                {
                    Id = produtoSelecionado.Id,
                    Nome = produtoSelecionado.Nome,
                    Tipo = produtoSelecionado.Tipo,
                    Rentabilidade = produtoSelecionado.Rentabilidade,
                    Risco = produtoSelecionado.Risco
                },
                ResultadoSimulacao = new ResultadoSimulacaoDto
                {
                    ValorFinal = valorFinal,
                    RentabilidadeEfetiva = rentabilidadeEfetiva,
                    PrazoMeses = request.PrazoMeses
                },
                DataSimulacao = simulacao.DataSimulacao
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular investimento");
            throw;
        }
    }

    public async Task<IEnumerable<SimulacaoDto>> ObterTodasSimulacoesAsync()
    {
        try
        {
            var simulacoes = await _context.Simulacoes
                .Include(s => s.Produto)
                .Include(s => s.Cliente)
                .OrderByDescending(s => s.DataSimulacao)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SimulacaoDto>>(simulacoes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter simulações");
            throw;
        }
    }

    public async Task<IEnumerable<SimulacaoPorProdutoDiaDto>> GetSimulacoesPorProdutoPorDiaAsync()
    {
        try
        {
            _logger.LogInformation("Obtendo simulações agrupadas por produto e dia");

            // Buscar dados do banco primeiro
            var simulacoes = await _context.Simulacoes
                .Include(s => s.Produto)
                .ToListAsync();

            // Fazer o agrupamento em memória
            var resultado = simulacoes
                .GroupBy(s => new
                {
                    ProdutoNome = s.Produto.Nome,
                    Data = s.DataSimulacao.Date
                })
                .Select(g => new SimulacaoPorProdutoDiaDto
                {
                    Produto = g.Key.ProdutoNome,
                    Data = g.Key.Data.ToString("yyyy-MM-dd"),
                    QuantidadeSimulacoes = g.Count(),
                    MediaValorFinal = g.Average(s => s.ValorFinal)
                })
                .OrderByDescending(x => x.Data)
                .ThenBy(x => x.Produto)
                .ToList();

            _logger.LogInformation("Retornando {Count} agrupamentos de simulações", resultado.Count);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter simulações por produto e dia");
            throw;
        }
    }
}