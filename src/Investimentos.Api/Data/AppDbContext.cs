using Microsoft.EntityFrameworkCore;
using Investimentos.Api.Models;

namespace Investimentos.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<PerfilRisco> PerfisRisco { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Simulacao> Simulacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Cpf).IsRequired().HasMaxLength(11);
            entity.Property(e => e.SaldoTotal).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.PerfilRisco)
                .WithMany(p => p.Clientes)
                .HasForeignKey(e => e.PerfilRiscoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuração da entidade PerfilRisco
        modelBuilder.Entity<PerfilRisco>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descricao).HasMaxLength(500);
        });

        // Configuração da entidade Produto
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Risco).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Rentabilidade).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ValorMinimoInvestimento).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PerfilRiscoRecomendado).HasMaxLength(50);
        });

        // Configuração da entidade Simulacao
        modelBuilder.Entity<Simulacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorInvestido).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ValorFinal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RentabilidadeEfetiva).HasColumnType("decimal(18,4)");
            
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Simulacoes)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Produto)
                .WithMany(p => p.Simulacoes)
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
