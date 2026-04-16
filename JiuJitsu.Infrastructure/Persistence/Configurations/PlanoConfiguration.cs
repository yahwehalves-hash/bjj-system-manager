using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class PlanoConfiguration : IEntityTypeConfiguration<Plano>
{
    public void Configure(EntityTypeBuilder<Plano> builder)
    {
        builder.ToTable("planos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(p => p.FilialId).HasColumnName("filial_id");
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(100).HasColumnName("nome");
        builder.Property(p => p.Descricao).HasMaxLength(500).HasColumnName("descricao");
        builder.Property(p => p.Valor).IsRequired().HasColumnType("decimal(10,2)").HasColumnName("valor");
        builder.Property(p => p.Periodicidade).HasConversion<string>().IsRequired().HasColumnName("periodicidade");
        builder.Property(p => p.Ativo).IsRequired().HasDefaultValue(true).HasColumnName("ativo");
        builder.Property(p => p.CriadoEm).IsRequired().HasColumnName("criado_em");
        builder.Property(p => p.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(p => new { p.FilialId, p.Ativo });
    }
}

public class MatriculaConfiguration : IEntityTypeConfiguration<Domain.Entities.Matricula>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Matricula> builder)
    {
        builder.ToTable("matriculas");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(m => m.AtletaId).IsRequired().HasColumnName("atleta_id");
        builder.Property(m => m.PlanoId).IsRequired().HasColumnName("plano_id");
        builder.Property(m => m.DataInicio).IsRequired().HasColumnName("data_inicio");
        builder.Property(m => m.DataFim).HasColumnName("data_fim");
        builder.Property(m => m.ValorCustomizado).HasColumnType("decimal(10,2)").HasColumnName("valor_customizado");
        builder.Property(m => m.Status).HasConversion<string>().IsRequired().HasColumnName("status");
        builder.Property(m => m.Observacao).HasMaxLength(500).HasColumnName("observacao");
        builder.Property(m => m.CriadoEm).IsRequired().HasColumnName("criado_em");
        builder.Property(m => m.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasOne(m => m.Atleta).WithMany().HasForeignKey(m => m.AtletaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Plano).WithMany().HasForeignKey(m => m.PlanoId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.AtletaId, m.Status });
    }
}
