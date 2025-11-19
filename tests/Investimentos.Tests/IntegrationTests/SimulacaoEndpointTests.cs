using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Investimentos.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Investimentos.Api.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Investimentos.Tests.IntegrationTests;

public class SimulacaoEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SimulacaoEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remover o DbContext existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Adicionar DbContext com InMemoryDatabase
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Build ServiceProvider e seed
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                db.Database.EnsureCreated();

                // Seed test data se necessário
                if (!db.Clientes.Any())
                {
                    SeedTestData(db);
                }
            });
        });

        _client = _factory.CreateClient();
    }

    private void SeedTestData(AppDbContext context)
    {
        var perfilRisco = new Investimentos.Api.Models.PerfilRisco
        {
            Id = 1,
            Nome = "Moderado",
            Descricao = "Perfil equilibrado",
            PontuacaoMinima = 41,
            PontuacaoMaxima = 70
        };

        var cliente = new Investimentos.Api.Models.Cliente
        {
            Id = 1,
            Nome = "Cliente Teste",
            Email = "teste@teste.com",
            Cpf = "12345678901",
            SaldoTotal = 50000,
            FrequenciaMovimentacoes = 5,
            PrefereLiquidez = false,
            PerfilRiscoId = 1
        };

        var produto = new Investimentos.Api.Models.Produto
        {
            Id = 1,
            Nome = "CDB Teste Integration",
            Tipo = "CDB",
            Rentabilidade = 0.12m,
            Risco = "Baixo",
            PrazoMinimoDias = 180,
            ValorMinimoInvestimento = 1000m,
            LiquidezImediata = false,
            PerfilRiscoRecomendado = "Conservador"
        };

        context.PerfisRisco.Add(perfilRisco);
        context.Clientes.Add(cliente);
        context.Produtos.Add(produto);
        context.SaveChanges();
    }

    [Fact]
    public async Task Post_SimularInvestimento_ComBypass_DeveRetornar200()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Adicionar header de bypass para desenvolvimento
        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Act
        var response = await _client.PostAsync("/api/simular-investimento", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_SimularInvestimento_DeveRetornarDadosCorretos()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Act
        var response = await _client.PostAsJsonAsync("/api/simular-investimento", request);
        var resultado = await response.Content.ReadFromJsonAsync<SimulacaoResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        resultado.Should().NotBeNull();
        resultado!.ProdutoValidado.Should().NotBeNull();
        resultado.ProdutoValidado.Nome.Should().Be("CDB Teste Integration");
        resultado.ProdutoValidado.Risco.Should().Be("Baixo");
        resultado.ResultadoSimulacao.Should().NotBeNull();
        resultado.ResultadoSimulacao.ValorFinal.Should().Be(11200m);
        resultado.ResultadoSimulacao.RentabilidadeEfetiva.Should().Be(0.12m);
        resultado.ResultadoSimulacao.PrazoMeses.Should().Be(12);
    }

    [Fact]
    public async Task Post_SimularInvestimento_DevePersistirNoBanco()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var simulacoesAntes = await context.Simulacoes.CountAsync();

        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = 5000m,
            PrazoMeses = 6,
            TipoProduto = "CDB"
        };

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Act
        var response = await _client.PostAsJsonAsync("/api/simular-investimento", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var simulacoesDepois = await context.Simulacoes.CountAsync();
        simulacoesDepois.Should().Be(simulacoesAntes + 1);

        var ultimaSimulacao = await context.Simulacoes
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        ultimaSimulacao.Should().NotBeNull();
        ultimaSimulacao!.ClienteId.Should().Be(1);
        ultimaSimulacao.ValorInvestido.Should().Be(5000m);
        ultimaSimulacao.PrazoMeses.Should().Be(6);
        ultimaSimulacao.ValorFinal.Should().BeGreaterThan(5000m);
    }

    [Fact]
    public async Task Post_SimularInvestimento_SemAutenticacao_DeveRetornar401()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        _client.DefaultRequestHeaders.Clear(); // Sem bypass nem token

        // Act
        var response = await _client.PostAsJsonAsync("/api/simular-investimento", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_SimularInvestimento_ComDadosInvalidos_DeveRetornar400()
    {
        // Arrange - Valor negativo
        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = -1000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Act
        var response = await _client.PostAsJsonAsync("/api/simular-investimento", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_Simulacoes_ComBypass_DeveRetornarLista()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Criar uma simulação primeiro
        var request = new SimulacaoRequest
        {
            ClienteId = 1,
            Valor = 10000m,
            PrazoMeses = 12,
            TipoProduto = "CDB"
        };
        await _client.PostAsJsonAsync("/api/simular-investimento", request);

        // Act
        var response = await _client.GetAsync("/api/simulacoes");
        var simulacoes = await response.Content.ReadFromJsonAsync<List<SimulacaoDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        simulacoes.Should().NotBeNull();
        simulacoes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Get_Health_DeveRetornar200()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
