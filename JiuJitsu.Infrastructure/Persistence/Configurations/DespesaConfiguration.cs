using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class DespesaConfiguration : IEntityTypeConfiguration<Despesa>
{
    public void Configure(EntityTypeBuilder<Despesa> builder)
    {
        builder.ToTable("despesas");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(d => d.FilialId)
            .IsRequired()
            .HasColumnName("filial_id");

        builder.HasOne(d => d.Filial)
            .WithMany()
            .HasForeignKey(d => d.FilialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.Descricao)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("descricao");

        builder.Property(d => d.Categoria)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("categoria");

        builder.Property(d => d.Subcategoria)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("subcategoria");

        builder.Property(d => d.Valor)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasColumnName("valor");

        builder.Property(d => d.DataCompetencia)
            .IsRequired()
            .HasColumnName("data_competencia");

        builder.Property(d => d.DataPagamento)
            .HasColumnName("data_pagamento");

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("status");

        builder.Property(d => d.FormaPagamento)
            .HasConversion<string>()
            .HasColumnName("forma_pagamento");

        builder.Property(d => d.Observacao)
            .HasMaxLength(500)
            .HasColumnName("observacao");

        builder.Property(d => d.RegistradoPorId)
            .HasColumnName("registrado_por_id");

        builder.Property(d => d.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");

        builder.Property(d => d.AtualizadoEm)
            .HasColumnName("atualizado_em");

        builder.HasIndex(d => new { d.FilialId, d.Status });
        builder.HasIndex(d => new { d.FilialId, d.DataCompetencia });
    }
}
