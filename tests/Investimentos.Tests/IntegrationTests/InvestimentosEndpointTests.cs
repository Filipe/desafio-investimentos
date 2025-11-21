using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Investimentos.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Investimentos.Tests.IntegrationTests;

public class InvestimentosEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public InvestimentosEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ObterInvestimentosPorCliente_DeveRetornarListaDeInvestimentos()
    {
        // Arrange
        int clienteId = 1; // Cliente do seed data
        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Act
        var response = await _client.GetAsync($"/api/investimentos/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var investimentos = await response.Content.ReadFromJsonAsync<List<InvestimentoDto>>();
        investimentos.Should().NotBeNull();
        investimentos.Should().BeOfType<List<InvestimentoDto>>();
        // Pode retornar lista vazia se não houver investimentos
    }

    [Fact]
    public async Task ObterInvestimentosPorCliente_DeveTerCamposCorretos()
    {
        // Arrange
        int clienteId = 1;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Act
        var response = await _client.GetAsync($"/api/investimentos/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var investimentos = await response.Content.ReadFromJsonAsync<List<InvestimentoDto>>();
        investimentos.Should().NotBeNull();
        
        // Se houver investimentos, valida estrutura
        if (investimentos != null && investimentos.Any())
        {
            var primeiroInvestimento = investimentos.First();
            primeiroInvestimento.Id.Should().BeGreaterThan(0);
            primeiroInvestimento.Tipo.Should().NotBeNullOrEmpty();
            primeiroInvestimento.Valor.Should().BeGreaterThan(0);
            primeiroInvestimento.Rentabilidade.Should().BeGreaterThanOrEqualTo(0);
            primeiroInvestimento.Data.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task ObterInvestimentosPorCliente_ClienteInexistente_DeveRetornarListaVazia()
    {
        // Arrange
        int clienteIdInexistente = 99999;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Act
        var response = await _client.GetAsync($"/api/investimentos/{clienteIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var investimentos = await response.Content.ReadFromJsonAsync<List<InvestimentoDto>>();
        investimentos.Should().NotBeNull();
        investimentos.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterInvestimentosPorCliente_DeveOrdenarPorDataDescrescente()
    {
        // Arrange
        int clienteId = 1;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("X-Debug-Bypass", "1");

        // Act
        var response = await _client.GetAsync($"/api/investimentos/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var investimentos = await response.Content.ReadFromJsonAsync<List<InvestimentoDto>>();
        investimentos.Should().NotBeNull();
        
        // Valida ordenação apenas se houver múltiplos investimentos
        if (investimentos != null && investimentos.Count > 1)
        {
            var datas = investimentos.Select(i => DateTime.Parse(i.Data)).ToList();
            datas.Should().BeInDescendingOrder();
        }
    }
}
