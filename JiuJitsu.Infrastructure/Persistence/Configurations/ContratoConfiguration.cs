using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class TemplateContratoConfiguration : IEntityTypeConfiguration<TemplateContrato>
{
    public void Configure(EntityTypeBuilder<TemplateContrato> builder)
    {
        builder.ToTable("templates_contrato");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(t => t.FilialId).HasColumnName("filial_id");
        builder.Property(t => t.Conteudo).IsRequired().HasColumnName("conteudo");
        builder.Property(t => t.Versao).IsRequired().HasDefaultValue(1).HasColumnName("versao");
        builder.Property(t => t.Ativo).IsRequired().HasDefaultValue(true).HasColumnName("ativo");
        builder.Property(t => t.CriadoEm).IsRequired().HasColumnName("criado_em");
    }
}

public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("contratos");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(c => c.AtletaId).IsRequired().HasColumnName("atleta_id");
        builder.Property(c => c.TemplateId).HasColumnName("template_id");
        builder.Property(c => c.HashDocumento).IsRequired().HasMaxLength(64).HasColumnName("hash_documento");
        builder.Property(c => c.IpAceite).HasMaxLength(45).HasColumnName("ip_aceite");
        builder.Property(c => c.DataAceite).IsRequired().HasColumnName("data_aceite");
        builder.Property(c => c.PdfBytes).IsRequired().HasColumnName("pdf_bytes");

        builder.HasIndex(c => c.AtletaId);
    }
}
