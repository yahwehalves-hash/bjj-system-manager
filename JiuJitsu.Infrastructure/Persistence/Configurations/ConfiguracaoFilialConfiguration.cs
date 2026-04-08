using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class ConfiguracaoFilialConfiguration : IEntityTypeConfiguration<ConfiguracaoFilial>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoFilial> builder)
    {
        builder.ToTable("configuracao_filial");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(c => c.FilialId)
            .IsRequired()
            .HasColumnName("filial_id");

        builder.HasIndex(c => c.FilialId).IsUnique();

        builder.HasOne(c => c.Filial)
            .WithOne()
            .HasForeignKey<ConfiguracaoFilial>(c => c.FilialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.ValorMensalidadePadrao)
            .HasColumnType("decimal(10,2)")
            .HasColumnName("valor_mensalidade_padrao");

        builder.Property(c => c.DiaVencimento)
            .HasColumnName("dia_vencimento");

        builder.Property(c => c.ToleranciaInadimplenciaDias)
            .HasColumnName("tolerancia_inadimplencia_dias");

        builder.Property(c => c.MultaAtrasoPercentual)
            .HasColumnType("decimal(7,6)")
            .HasColumnName("multa_atraso_percentual");

        builder.Property(c => c.JurosDiarioPercentual)
            .HasColumnType("decimal(7,6)")
            .HasColumnName("juros_diario_percentual");

        builder.Property(c => c.DescontoAntecipacaoPercentual)
            .HasColumnType("decimal(7,6)")
            .HasColumnName("desconto_antecipacao_percentual");

        builder.Property(c => c.AtualizadoEm)
            .IsRequired()
            .HasColumnName("atualizado_em");

        builder.Property(c => c.AtualizadoPorId)
            .HasColumnName("atualizado_por_id");
    }
}
