using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    public DbSet<Atleta>  Atletas  => Set<Atleta>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public AppDbContext(DbContextOptions<AppDbContext> opcoes) : base(opcoes) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Carrega todas as configurações do assembly de Infrastructure
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
