using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class RegistroPresencaConfiguration : IEntityTypeConfiguration<RegistroPresenca>
{
    public void Configure(EntityTypeBuilder<RegistroPresenca> builder)
    {
        builder.ToTable("registros_presenca");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever().HasColumnName("id");

        builder.Property(r => r.AtletaId).IsRequired().HasColumnName("atleta_id");
        builder.Property(r => r.TurmaId).IsRequired().HasColumnName("turma_id");
        builder.Property(r => r.FilialId).IsRequired().HasColumnName("filial_id");
        builder.Property(r => r.DataHora).IsRequired().HasColumnName("data_hora");
        builder.Property(r => r.Origem).HasConversion<string>().IsRequired().HasColumnName("origem");
        builder.Property(r => r.RegistradoPor).HasColumnName("registrado_por");

        builder.HasOne(r => r.Atleta)
            .WithMany()
            .HasForeignKey(r => r.AtletaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Turma)
            .WithMany()
            .HasForeignKey(r => r.TurmaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.AtletaId, r.TurmaId, r.DataHora });
        builder.HasIndex(r => new { r.FilialId, r.DataHora });
        builder.HasIndex(r => r.AtletaId);
    }
}
