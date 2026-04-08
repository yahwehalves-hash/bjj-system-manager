using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class ConfiguracaoGlobalConfiguration : IEntityTypeConfiguration<ConfiguracaoGlobal>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoGlobal> builder)
    {
        builder.ToTable("configuracao_global");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(c => c.ValorMensalidadePadrao)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasColumnName("valor_mensalidade_padrao");

        builder.Property(c => c.DiaVencimento)
            .IsRequired()
            .HasColumnName("dia_vencimento");

        builder.Property(c => c.ToleranciaInadimplenciaDias)
            .IsRequired()
            .HasColumnName("tolerancia_inadimplencia_dias");

        builder.Property(c => c.MultaAtrasoPercentual)
            .IsRequired()
            .HasColumnType("decimal(7,6)")
            .HasColumnName("multa_atraso_percentual");

        builder.Property(c => c.JurosDiarioPercentual)
            .IsRequired()
            .HasColumnType("decimal(7,6)")
            .HasColumnName("juros_diario_percentual");

        builder.Property(c => c.DescontoAntecipacaoPercentual)
            .IsRequired()
            .HasColumnType("decimal(7,6)")
            .HasColumnName("desconto_antecipacao_percentual");

        builder.Property(c => c.AtualizadoEm)
            .IsRequired()
            .HasColumnName("atualizado_em");

        builder.Property(c => c.AtualizadoPorId)
            .HasColumnName("atualizado_por_id");
    }
}
