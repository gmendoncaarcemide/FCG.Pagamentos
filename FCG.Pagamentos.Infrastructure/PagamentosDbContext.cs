using FCG.Pagamentos.Domain.Pagamentos.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Pagamentos.Infrastructure;

public class PagamentosDbContext : DbContext
{
    public PagamentosDbContext(DbContextOptions<PagamentosDbContext> options) : base(options)
    {
    }

    public DbSet<Transacao> Transacoes { get; set; }
    public DbSet<Reembolso> Reembolsos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).HasPrecision(18, 2);
            entity.Property(e => e.Moeda).HasMaxLength(3);
            entity.Property(e => e.Referencia).HasMaxLength(100);
            entity.Property(e => e.DetalhesPagamento).HasMaxLength(1000);
            
            entity.HasIndex(e => e.Referencia).IsUnique();
            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Reembolso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorReembolso).HasPrecision(18, 2);
            entity.Property(e => e.Motivo).HasMaxLength(500);
            
            entity.HasOne<Transacao>()
                .WithMany()
                .HasForeignKey(e => e.TransacaoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
} 