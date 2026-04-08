using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class MensalidadeConfiguration : IEntityTypeConfiguration<Mensalidade>
{
    public void Configure(EntityTypeBuilder<Mensalidade> builder)
    {
        builder.ToTable("mensalidades");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(m => m.AtletaId)
            .IsRequired()
            .HasColumnName("atleta_id");

        builder.Property(m => m.FilialId)
            .IsRequired()
            .HasColumnName("filial_id");

        // Um atleta só pode ter uma mensalidade por competência (mês/ano)
        builder.HasIndex(m => new { m.AtletaId, m.Competencia }).IsUnique();

        builder.HasOne(m => m.Atleta)
            .WithMany()
            .HasForeignKey(m => m.AtletaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Filial)
            .WithMany()
            .HasForeignKey(m => m.FilialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.Competencia)
            .IsRequired()
            .HasColumnName("competencia");

        builder.Property(m => m.Valor)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasColumnName("valor");

        builder.Property(m => m.ValorPago)
            .HasColumnType("decimal(10,2)")
            .HasColumnName("valor_pago");

        builder.Property(m => m.DataVencimento)
            .IsRequired()
            .HasColumnName("data_vencimento");

        builder.Property(m => m.DataPagamento)
            .HasColumnName("data_pagamento");

        // Enums armazenados como texto para facilitar leitura no banco
        builder.Property(m => m.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("status");

        builder.Property(m => m.FormaPagamento)
            .HasConversion<string>()
            .HasColumnName("forma_pagamento");

        builder.Property(m => m.Observacao)
            .HasMaxLength(500)
            .HasColumnName("observacao");

        builder.Property(m => m.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");

        builder.Property(m => m.AtualizadoEm)
            .HasColumnName("atualizado_em");

        // Índices para consultas frequentes de dashboard e job de inadimplência
        builder.HasIndex(m => new { m.FilialId, m.Status });
        builder.HasIndex(m => new { m.DataVencimento, m.Status });
    }
}
