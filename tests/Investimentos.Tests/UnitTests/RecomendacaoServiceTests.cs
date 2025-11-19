using FluentAssertions;
using Investimentos.Api.Models;
using Investimentos.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Investimentos.Tests.UnitTests;

public class RecomendacaoServiceTests
{
    private readonly RecomendacaoService _service;
    private readonly Mock<ILogger<RecomendacaoService>> _loggerMock;

    public RecomendacaoServiceTests()
    {
        _loggerMock = new Mock<ILogger<RecomendacaoService>>();
        _service = new RecomendacaoService(_loggerMock.Object);
    }

    [Theory]
    [InlineData(10000, 2, true, "Conservador", 10)] // Baixo volume, baixa freq, prefere liquidez: (10000/100000)*40=4, 2*3=6, 0 => 10
    [InlineData(50000, 5, false, "Moderado", 65)] // Volume médio, freq média, não prefere liquidez: (50000/100000)*40=20, 5*3=15, 30 => 65
    [InlineData(100000, 10, false, "Agressivo", 100)] // Alto volume, alta freq, não prefere liquidez: 40, 30, 30 => 100
    [InlineData(5000, 1, true, "Conservador", 5)] // Volume muito baixo: (5000/100000)*40=2, 1*3=3, 0 => 5
    [InlineData(80000, 8, false, "Agressivo", 86)] // Alto volume, alta freq: (80000/100000)*40=32, 8*3=24, 30 => 86
    public void CalcularPerfil_DeveMappearPontuacaoParaPerfilCorretamente(
        decimal saldoTotal,
        int frequenciaMovimentacoes,
        bool prefereLiquidez,
        string perfilEsperado,
        int pontuacaoEsperada)
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "Teste",
            Email = "teste@teste.com",
            Cpf = "12345678901",
            SaldoTotal = saldoTotal,
            FrequenciaMovimentacoes = frequenciaMovimentacoes,
            PrefereLiquidez = prefereLiquidez
        };

        // Act
        var (perfil, pontuacao) = _service.CalcularPerfil(cliente);

        // Assert
        perfil.Should().Be(perfilEsperado);
        pontuacao.Should().Be(pontuacaoEsperada);
    }

    [Fact]
    public void CalcularPerfil_Conservador_PontuacaoEntre0e40()
    {
        // Arrange - Cliente conservador
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "Conservador",
            Email = "conservador@teste.com",
            Cpf = "12345678901",
            SaldoTotal = 20000m, // 8 pontos
            FrequenciaMovimentacoes = 3, // 9 pontos
            PrefereLiquidez = true // 0 pontos
        };

        // Act
        var (perfil, pontuacao) = _service.CalcularPerfil(cliente);

        // Assert
        perfil.Should().Be("Conservador");
        pontuacao.Should().BeInRange(0, 40);
        pontuacao.Should().Be(17); // 8 + 9 + 0 = 17
    }

    [Fact]
    public void CalcularPerfil_Moderado_PontuacaoEntre41e70()
    {
        // Arrange - Cliente moderado
        var cliente = new Cliente
        {
            Id = 2,
            Nome = "Moderado",
            Email = "moderado@teste.com",
            Cpf = "12345678902",
            SaldoTotal = 50000m, // 20 pontos
            FrequenciaMovimentacoes = 10, // 30 pontos
            PrefereLiquidez = true // 0 pontos
        };

        // Act
        var (perfil, pontuacao) = _service.CalcularPerfil(cliente);

        // Assert
        perfil.Should().Be("Moderado");
        pontuacao.Should().BeInRange(41, 70);
        pontuacao.Should().Be(50); // 20 + 30 + 0 = 50
    }

    [Fact]
    public void CalcularPerfil_Agressivo_PontuacaoEntre71e100()
    {
        // Arrange - Cliente agressivo
        var cliente = new Cliente
        {
            Id = 3,
            Nome = "Agressivo",
            Email = "agressivo@teste.com",
            Cpf = "12345678903",
            SaldoTotal = 150000m, // 40 pontos (capped)
            FrequenciaMovimentacoes = 15, // 30 pontos (capped)
            PrefereLiquidez = false // 30 pontos
        };

        // Act
        var (perfil, pontuacao) = _service.CalcularPerfil(cliente);

        // Assert
        perfil.Should().Be("Agressivo");
        pontuacao.Should().BeInRange(71, 100);
        pontuacao.Should().Be(100); // 40 + 30 + 30 = 100
    }

    [Fact]
    public void CalcularPerfil_DeveLimitarPontuacaoMaximaEm100()
    {
        // Arrange - Cliente com valores muito altos
        var cliente = new Cliente
        {
            Id = 4,
            Nome = "Super Agressivo",
            Email = "super@teste.com",
            Cpf = "12345678904",
            SaldoTotal = 1000000m, // Muito alto
            FrequenciaMovimentacoes = 100, // Muito alto
            PrefereLiquidez = false
        };

        // Act
        var (perfil, pontuacao) = _service.CalcularPerfil(cliente);

        // Assert
        pontuacao.Should().BeLessThanOrEqualTo(100);
        pontuacao.Should().Be(100);
        perfil.Should().Be("Agressivo");
    }

    [Fact]
    public void CalcularPerfil_PreferenciaLiquidez_DeveImpactarPontuacao()
    {
        // Arrange - Dois clientes iguais exceto preferência de liquidez
        var clienteComLiquidez = new Cliente
        {
            Id = 1,
            Nome = "Com Liquidez",
            Email = "com@teste.com",
            Cpf = "12345678901",
            SaldoTotal = 30000m,
            FrequenciaMovimentacoes = 5,
            PrefereLiquidez = true
        };

        var clienteSemLiquidez = new Cliente
        {
            Id = 2,
            Nome = "Sem Liquidez",
            Email = "sem@teste.com",
            Cpf = "12345678902",
            SaldoTotal = 30000m,
            FrequenciaMovimentacoes = 5,
            PrefereLiquidez = false
        };

        // Act
        var (perfilCom, pontuacaoCom) = _service.CalcularPerfil(clienteComLiquidez);
        var (perfilSem, pontuacaoSem) = _service.CalcularPerfil(clienteSemLiquidez);

        // Assert
        pontuacaoSem.Should().Be(pontuacaoCom + 30);
        pontuacaoCom.Should().Be(27); // 12 + 15 + 0
        pontuacaoSem.Should().Be(57); // 12 + 15 + 30
    }

    [Theory]
    [InlineData(0)] // Pontuação 0 = Conservador
    [InlineData(20)] // Pontuação 20 = Conservador
    [InlineData(40)] // Pontuação 40 = Conservador (limite)
    public void CalcularPerfil_LimiteConservador_Pontuacao0a40(int pontuacaoAlvo)
    {
        // Arrange - Construir cliente que resulte na pontuação alvo
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "Teste",
            Email = "teste@teste.com",
            Cpf = "12345678901",
            SaldoTotal = (decimal)(pontuacaoAlvo * 1000),
            FrequenciaMovimentacoes = 0,
            PrefereLiquidez = true
        };

        // Act
        var (perfil, pontuacao) = _service.CalcularPerfil(cliente);

        // Assert
        perfil.Should().Be("Conservador");
    }

    [Theory]
    [InlineData(41)] // Pontuação 41 = Moderado (início): volume=11 + freq=30 + liq=0 = 41
    [InlineData(55)] // Pontuação 55 = Moderado (meio): volume=25 + freq=30 + liq=0 = 55
    [InlineData(70)] // Pontuação 70 = Moderado (limite): volume=40 + freq=30 + liq=0 = 70
    public void CalcularPerfil_LimiteModerado_Pontuacao41a70(int pontuacaoAlvo)
    {
        // Arrange
        // Para atingir pontuação alvo:
        // volume = pontuacaoAlvo - 30 (assumindo freq=10=30pts, liq=0)
        // saldo = (volume/40) * 100000
        int pontosVolume = pontuacaoAlvo - 30; // Considerando freq=10 dá 30 pontos
        decimal saldoTotal = (decimal)pontosVolume * 100000 / 40;
        
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "Teste",
            Email = "teste@teste.com",
            Cpf = "12345678901",
            SaldoTotal = saldoTotal,
            FrequenciaMovimentacoes = 10, // 10 * 3 = 30 pontos
            PrefereLiquidez = true // 0 pontos
        };

        // Act
        var (perfil, pontuacao) = _service.CalcularPerfil(cliente);

        // Assert
        perfil.Should().Be("Moderado");
        pontuacao.Should().Be(pontuacaoAlvo);
    }

    [Theory]
    [InlineData(73)] // Pontuação 73 = Agressivo (início): volume=40 + freq=3 (1*3) + liq=30 = 73
    [InlineData(85)] // Pontuação 85 = Agressivo (meio): volume=40 + freq=15 (5*3) + liq=30 = 85
    [InlineData(100)] // Pontuação 100 = Agressivo (máximo): volume=40 + freq=30 (10*3) + liq=30 = 100
    public void CalcularPerfil_LimiteAgressivo_Pontuacao71a100(int pontuacaoAlvo)
    {
        // Arrange
        // Para atingir pontuação alvo com volume=40 e liq=30:
        // freq = (pontuacaoAlvo - 40 - 30) / 3
        int frequencia = (pontuacaoAlvo - 40 - 30) / 3;
        
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "Teste",
            Email = "teste@teste.com",
            Cpf = "12345678901",
            SaldoTotal = 100000m, // Volume máximo = 40 pontos
            FrequenciaMovimentacoes = frequencia,
            PrefereLiquidez = false // 30 pontos
        };

        // Act
        var (perfil, pontuacao) = _service.CalcularPerfil(cliente);

        // Assert
        perfil.Should().Be("Agressivo");
        pontuacao.Should().Be(pontuacaoAlvo);
    }
}
