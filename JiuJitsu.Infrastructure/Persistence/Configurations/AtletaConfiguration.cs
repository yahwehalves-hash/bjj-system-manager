using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JiuJitsu.Infrastructure.Persistence.Configurations;

public class AtletaConfiguration : IEntityTypeConfiguration<Atleta>
{
    public void Configure(EntityTypeBuilder<Atleta> builder)
    {
        builder.ToTable("atletas");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(a => a.NomeCompleto)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("nome_completo");

        // CPF é um Value Object — armazenado como coluna simples
        builder.OwnsOne(a => a.Cpf, cpf =>
        {
            cpf.Property(c => c.Valor)
                .HasColumnName("cpf")
                .HasMaxLength(11)
                .IsRequired();
            cpf.HasIndex(c => c.Valor).IsUnique();
        });

        // Email é um Value Object — armazenado como coluna simples
        builder.OwnsOne(a => a.Email, email =>
        {
            email.Property(e => e.Valor)
                .HasColumnName("email")
                .HasMaxLength(254)
                .IsRequired();
            email.HasIndex(e => e.Valor).IsUnique();
        });

        builder.Property(a => a.DataNascimento)
            .IsRequired()
            .HasColumnName("data_nascimento");

        // Enum armazenado como texto para facilitar leitura no banco
        builder.Property(a => a.Faixa)
            .HasConversion<string>()
            .IsRequired()
            .HasColumnName("faixa");

        builder.Property(a => a.Grau)
            .HasConversion<int>()
            .IsRequired()
            .HasColumnName("grau");

        builder.Property(a => a.DataUltimaGraduacao)
            .IsRequired()
            .HasColumnName("data_ultima_graduacao");

        builder.Property(a => a.Ativo)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("ativo");

        builder.Property(a => a.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");

        builder.Property(a => a.AtualizadoEm)
            .HasColumnName("atualizado_em");

        // Filtro global de soft delete — consultas só retornam atletas ativos por padrão
        builder.HasQueryFilter(a => a.Ativo);
    }
}
