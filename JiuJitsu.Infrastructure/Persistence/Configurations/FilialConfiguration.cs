using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class FilialConfiguration : IEntityTypeConfiguration<Filial>
{
    public void Configure(EntityTypeBuilder<Filial> builder)
    {
        builder.ToTable("filiais");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(f => f.Nome)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("nome");

        builder.Property(f => f.Endereco)
            .HasMaxLength(500)
            .HasColumnName("endereco");

        builder.Property(f => f.Cnpj)
            .HasMaxLength(14)
            .HasColumnName("cnpj");

        builder.HasIndex(f => f.Cnpj)
            .IsUnique()
            .HasFilter("cnpj IS NOT NULL");

        builder.Property(f => f.Telefone)
            .HasMaxLength(20)
            .HasColumnName("telefone");

        builder.Property(f => f.Ativo)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("ativo");

        builder.Property(f => f.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");
    }
}
