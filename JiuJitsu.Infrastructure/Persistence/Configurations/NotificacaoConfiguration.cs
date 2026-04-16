using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class TemplateNotificacaoConfiguration : IEntityTypeConfiguration<TemplateNotificacao>
{
    public void Configure(EntityTypeBuilder<TemplateNotificacao> builder)
    {
        builder.ToTable("templates_notificacao");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(t => t.Evento).IsRequired().HasMaxLength(100).HasColumnName("evento");
        builder.Property(t => t.Canal).IsRequired().HasMaxLength(20).HasColumnName("canal");
        builder.Property(t => t.Mensagem).IsRequired().HasColumnName("mensagem");
        builder.Property(t => t.Ativo).IsRequired().HasDefaultValue(true).HasColumnName("ativo");
        builder.Property(t => t.CriadoEm).IsRequired().HasColumnName("criado_em");
    }
}

public class HistoricoNotificacaoConfiguration : IEntityTypeConfiguration<HistoricoNotificacao>
{
    public void Configure(EntityTypeBuilder<HistoricoNotificacao> builder)
    {
        builder.ToTable("historico_notificacoes");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(h => h.AtletaId).IsRequired().HasColumnName("atleta_id");
        builder.Property(h => h.Evento).IsRequired().HasMaxLength(100).HasColumnName("evento");
        builder.Property(h => h.Canal).IsRequired().HasMaxLength(20).HasColumnName("canal");
        builder.Property(h => h.Status).IsRequired().HasMaxLength(20).HasColumnName("status");
        builder.Property(h => h.Detalhe).HasColumnName("detalhe");
        builder.Property(h => h.EnviadoEm).IsRequired().HasColumnName("enviado_em");
    }
}
