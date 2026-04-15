using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class HistoricoGraduacaoConfiguration : IEntityTypeConfiguration<HistoricoGraduacao>
{
    public void Configure(EntityTypeBuilder<HistoricoGraduacao> builder)
    {
        builder.ToTable("historico_graduacoes");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(h => h.AtletaId)
            .IsRequired()
            .HasColumnName("atleta_id");

        builder.Property(h => h.Faixa)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("faixa");

        builder.Property(h => h.Grau)
            .HasConversion<int>()
            .IsRequired()
            .HasColumnName("grau");

        builder.Property(h => h.DataInicio)
            .IsRequired()
            .HasColumnName("data_inicio");

        builder.Property(h => h.DataFim)
            .HasColumnName("data_fim");

        builder.HasOne(h => h.Atleta)
            .WithMany(a => a.Historico)
            .HasForeignKey(h => h.AtletaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => new { h.AtletaId, h.DataInicio });
    }
}
