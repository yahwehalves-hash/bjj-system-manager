using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(u => u.FilialId)
            .HasColumnName("filial_id");

        builder.HasOne(u => u.Filial)
            .WithMany()
            .HasForeignKey(u => u.FilialId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.Property(u => u.Nome)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("nome");

        builder.Property(u => u.Email)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("email");

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.SenhaHash)
            .IsRequired()
            .HasColumnName("senha_hash");

        builder.Property(u => u.Role)
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnName("role");

        builder.Property(u => u.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");
    }
}
