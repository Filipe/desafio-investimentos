using Investimentos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Investimentos.Api.Data;

public static class SeedData
{
    public static void Seed(AppDbContext context)
    {
        // Garante que o banco de dados está criado
        context.Database.EnsureCreated();

        // Verifica se já existem dados para evitar duplicação
        if (context.Produtos.Any() || context.Clientes.Any())
        {
            return; // Dados já foram inseridos
        }

        // Inserir Perfis de Risco
        var perfis = new[]
        {
            new PerfilRisco
            {
                Nome = "Conservador",
                Descricao = "Perfil com baixa tolerância a risco, foco em liquidez e segurança.",
                PontuacaoMinima = 0,
                PontuacaoMaxima = 40
            },
            new PerfilRisco
            {
                Nome = "Moderado",
                Descricao = "Perfil equilibrado entre segurança e rentabilidade.",
                PontuacaoMinima = 41,
                PontuacaoMaxima = 70
            },
            new PerfilRisco
            {
                Nome = "Agressivo",
                Descricao = "Perfil com alta tolerância a risco, busca por alta rentabilidade.",
                PontuacaoMinima = 71,
                PontuacaoMaxima = 100
            }
        };
        context.PerfisRisco.AddRange(perfis);
        context.SaveChanges();

        // Inserir Produtos de Investimento
        var produtos = new[]
        {
            new Produto
            {
                Nome = "CDB CAIXA 2026",
                Tipo = "CDB",
                Rentabilidade = 0.12m,
                Risco = "Baixo",
                PrazoMinimoDias = 180,
                ValorMinimoInvestimento = 1000.00m,
                LiquidezImediata = false,
                PerfilRiscoRecomendado = "Conservador",
                DataCriacao = DateTime.UtcNow
            },
            new Produto
            {
                Nome = "LCI CAIXA",
                Tipo = "LCI",
                Rentabilidade = 0.15m,
                Risco = "Médio",
                PrazoMinimoDias = 90,
                ValorMinimoInvestimento = 2000.00m,
                LiquidezImediata = false,
                PerfilRiscoRecomendado = "Moderado",
                DataCriacao = DateTime.UtcNow
            },
            new Produto
            {
                Nome = "Fundo Multimercado XPTO",
                Tipo = "Fundo",
                Rentabilidade = 0.18m,
                Risco = "Alto",
                PrazoMinimoDias = 0,
                ValorMinimoInvestimento = 500.00m,
                LiquidezImediata = true,
                PerfilRiscoRecomendado = "Agressivo",
                DataCriacao = DateTime.UtcNow
            }
        };
        context.Produtos.AddRange(produtos);
        context.SaveChanges();

        // Inserir Cliente de Exemplo
        var perfilModerado = context.PerfisRisco.First(p => p.Nome == "Moderado");
        var cliente = new Cliente
        {
            Nome = "João da Silva",
            Email = "joao.silva@example.com",
            Cpf = "12345678901",
            DataCadastro = DateTime.UtcNow,
            SaldoTotal = 50000.00m,
            FrequenciaMovimentacoes = 5,
            PrefereLiquidez = false,
            PerfilRiscoId = perfilModerado.Id
        };
        context.Clientes.Add(cliente);
        context.SaveChanges();

        Console.WriteLine("✅ Banco de dados criado e dados iniciais inseridos com sucesso!");
    }
}
