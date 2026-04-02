using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Nome).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.SenhaHash).IsRequired();
        builder.Property(u => u.Role).HasMaxLength(50).IsRequired();
    }
}
