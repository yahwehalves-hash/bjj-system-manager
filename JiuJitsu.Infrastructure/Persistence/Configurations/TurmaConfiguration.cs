using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class TurmaConfiguration : IEntityTypeConfiguration<Turma>
{
    public void Configure(EntityTypeBuilder<Turma> builder)
    {
        builder.ToTable("turmas");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever().HasColumnName("id");

        builder.Property(t => t.FilialId).IsRequired().HasColumnName("filial_id");

        builder.HasOne(t => t.Filial)
            .WithMany()
            .HasForeignKey(t => t.FilialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Nome).IsRequired().HasMaxLength(150).HasColumnName("nome");
        builder.Property(t => t.ProfessorId).HasColumnName("professor_id");
        builder.Property(t => t.DiasSemana).IsRequired().HasMaxLength(100).HasColumnName("dias_semana");
        builder.Property(t => t.Horario).IsRequired().HasMaxLength(10).HasColumnName("horario");
        builder.Property(t => t.CapacidadeMaxima).IsRequired().HasColumnName("capacidade_maxima");
        builder.Property(t => t.Ativo).IsRequired().HasDefaultValue(true).HasColumnName("ativo");
        builder.Property(t => t.CriadoEm).IsRequired().HasColumnName("criado_em");
        builder.Property(t => t.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasMany(t => t.AtletasTurmas)
            .WithOne(at => at.Turma)
            .HasForeignKey(at => at.TurmaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(t => t.Ativo);
    }
}

public class AtletaTurmaConfiguration : IEntityTypeConfiguration<AtletaTurma>
{
    public void Configure(EntityTypeBuilder<AtletaTurma> builder)
    {
        builder.ToTable("atletas_turmas");
        builder.HasKey(at => new { at.AtletaId, at.TurmaId });

        builder.Property(at => at.AtletaId).HasColumnName("atleta_id");
        builder.Property(at => at.TurmaId).HasColumnName("turma_id");
        builder.Property(at => at.VinculadoEm).HasColumnName("vinculado_em");

        builder.HasOne(at => at.Atleta)
            .WithMany()
            .HasForeignKey(at => at.AtletaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
