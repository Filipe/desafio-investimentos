using FluentAssertions;
using Investimentos.Api.Data;
using Investimentos.Api.DTOs;
using Investimentos.Api.Models;
using Investimentos.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Investimentos.Tests.UnitTests;

public class SimulacaoServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SimulacaoService _service;
    private readonly Mock<ILogger<SimulacaoService>> _loggerMock;

    public SimulacaoServiceTests()
    {
        // Configurar InMemoryDatabase
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<SimulacaoService>>();

        // Configurar AutoMapper
        var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new Investimentos.Api.Mappings.MappingProfile());
        });
        var mapper = mapperConfig.CreateMapper();

        _service = new SimulacaoService(_context, mapper, _loggerMock.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "Cliente Teste",
            Email = "teste@teste.com",
            Cpf = "12345678901",
            SaldoTotal = 50000,
            FrequenciaMovimentacoes = 5,
            PrefereLiquidez = false
        };

        var produto = new Produto
        {
            Id = 1,
            Nome = "CDB Teste",
            Tipo = "CDB",
            Rentabilidade = 0.12m, // 12% ao ano
            Risco = "Baixo",
            PrazoMinimoDias = 180,
            ValorMinimoInvestimento = 1000m,
            LiquidezImediata = false,
            PerfilRiscoRecomendado = "Conservador"
        };

        _context.Clientes.Add(cliente);
        _context.Produtos.Add(produto);
        _context.SaveChanges();
    }

    [Fact]
    public async Task SimularInvestimentoAsync_DeveCalcularValorFinalCorretamente()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        // Act
        var resultado = await _service.SimularInvestimentoAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.ResultadoSimulacao.ValorFinal.Should().Be(11200m); // 10000 * (1 + 0.12)^1
        resultado.ResultadoSimulacao.RentabilidadeEfetiva.Should().Be(0.12m);
        resultado.ResultadoSimulacao.PrazoMeses.Should().Be(12);
    }

    [Fact]
    public async Task SimularInvestimentoAsync_DevePersistirSimulacaoNoBanco()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = 5000m,
            PrazoMeses = 6,
            TipoProduto = "CDB"
        };

        var simulacoesAntes = await _context.Simulacoes.CountAsync();

        // Act
        await _service.SimularInvestimentoAsync(request);

        // Assert
        var simulacoesDepois = await _context.Simulacoes.CountAsync();
        simulacoesDepois.Should().Be(simulacoesAntes + 1);

        var simulacao = await _context.Simulacoes
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        simulacao.Should().NotBeNull();
        simulacao!.ClienteId.Should().Be(1);
        simulacao.ProdutoId.Should().Be(1);
        simulacao.ValorInvestido.Should().Be(5000m);
        simulacao.PrazoMeses.Should().Be(6);
    }

    [Theory]
    [InlineData(10000, 12, 11200.00)] // 12 meses = 1 ano: 10000 * 1.12
    [InlineData(5000, 6, 5291.50)] // 6 meses = 0.5 ano: 5000 * 1.12^0.5 = 5000 * 1.058300524 = 5291.50
    [InlineData(15000, 24, 18816.00)] // 24 meses = 2 anos: 15000 * 1.12^2 = 15000 * 1.2544 = 18816
    public async Task SimularInvestimentoAsync_DeveCalcularValorFinalParaDiferentesPrazos(
        decimal valorInicial, int prazoMeses, double valorFinalEsperado)
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = valorInicial,
            PrazoMeses = prazoMeses,
            TipoProduto = "CDB"
        };

        // Act
        var resultado = await _service.SimularInvestimentoAsync(request);

        // Assert
        resultado.ResultadoSimulacao.ValorFinal.Should().BeApproximately((decimal)valorFinalEsperado, 0.01m);
    }

    [Fact]
    public async Task SimularInvestimentoAsync_DeveSelecionarProdutoComMenorRisco()
    {
        // Arrange - Adicionar produto de risco mais alto
        var produtoAltoRisco = new Produto
        {
            Id = 2,
            Nome = "Fundo Teste",
            Tipo = "CDB", // Mesmo tipo
            Rentabilidade = 0.18m,
            Risco = "Alto",
            PrazoMinimoDias = 30,
            ValorMinimoInvestimento = 100m,
            LiquidezImediata = true,
            PerfilRiscoRecomendado = "Agressivo"
        };
        _context.Produtos.Add(produtoAltoRisco);
        await _context.SaveChangesAsync();

        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        // Act
        var resultado = await _service.SimularInvestimentoAsync(request);

        // Assert
        resultado.ProdutoValidado.Risco.Should().Be("Baixo");
        resultado.ProdutoValidado.Nome.Should().Be("CDB Teste");
    }

    [Fact]
    public async Task ObterTodasSimulacoesAsync_DeveRetornarSimulacoesOrdenadas()
    {
        // Arrange - Criar múltiplas simulações
        var simulacao1 = new Simulacao
        {
            ClienteId = 1,
            ProdutoId = 1,
            ValorInvestido = 1000m,
            ValorFinal = 1120m,
            RentabilidadeEfetiva = 0.12m,
            PrazoMeses = 12,
            DataSimulacao = DateTime.UtcNow.AddDays(-2),
            TempoRespostaMs = 100
        };

        var simulacao2 = new Simulacao
        {
            ClienteId = 1,
            ProdutoId = 1,
            ValorInvestido = 2000m,
            ValorFinal = 2240m,
            RentabilidadeEfetiva = 0.12m,
            PrazoMeses = 12,
            DataSimulacao = DateTime.UtcNow.AddDays(-1),
            TempoRespostaMs = 150
        };

        _context.Simulacoes.AddRange(simulacao1, simulacao2);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _service.ObterTodasSimulacoesAsync();
        var lista = resultado.ToList();

        // Assert
        lista.Should().HaveCount(2);
        lista[0].ValorInvestido.Should().Be(2000m); // Mais recente primeiro
        lista[1].ValorInvestido.Should().Be(1000m);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
