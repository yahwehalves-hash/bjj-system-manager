using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class RegraGraduacaoConfiguration : IEntityTypeConfiguration<RegraGraduacao>
{
    public void Configure(EntityTypeBuilder<RegraGraduacao> builder)
    {
        builder.ToTable("regras_graduacao");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(r => r.FilialId).HasColumnName("filial_id");
        builder.Property(r => r.Faixa).HasConversion<string>().IsRequired().HasColumnName("faixa");
        builder.Property(r => r.Grau).HasConversion<int>().IsRequired().HasColumnName("grau");
        builder.Property(r => r.TempoMinimoMeses).IsRequired().HasColumnName("tempo_minimo_meses");
        builder.Property(r => r.AtualizadoEm).IsRequired().HasColumnName("atualizado_em");

        builder.HasIndex(r => new { r.FilialId, r.Faixa, r.Grau }).IsUnique();
    }
}
