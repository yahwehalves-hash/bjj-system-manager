using JiuJitsu.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Context;

public class AppDbContext : DbContext
{
    public DbSet<Atleta>               Atletas              => Set<Atleta>();
    public DbSet<Usuario>              Usuarios             => Set<Usuario>();
    public DbSet<Filial>               Filiais              => Set<Filial>();
    public DbSet<ConfiguracaoGlobal>   ConfiguracaoGlobal   => Set<ConfiguracaoGlobal>();
    public DbSet<ConfiguracaoFilial>   ConfiguracaoFilial   => Set<ConfiguracaoFilial>();
    public DbSet<Mensalidade>          Mensalidades         => Set<Mensalidade>();
    public DbSet<Despesa>              Despesas             => Set<Despesa>();
    public DbSet<HistoricoGraduacao>   HistoricoGraduacoes  => Set<HistoricoGraduacao>();
    public DbSet<Turma>                Turmas               => Set<Turma>();
    public DbSet<AtletaTurma>          AtletasTurmas        => Set<AtletaTurma>();
    public DbSet<RegraGraduacao>       RegrasGraduacao      => Set<RegraGraduacao>();
    public DbSet<TemplateNotificacao>  TemplatesNotificacao => Set<TemplateNotificacao>();
    public DbSet<HistoricoNotificacao> HistoricoNotificacoes => Set<HistoricoNotificacao>();
    public DbSet<TemplateContrato>     TemplatesContrato    => Set<TemplateContrato>();
    public DbSet<Contrato>             Contratos            => Set<Contrato>();
    public DbSet<Plano>                Planos               => Set<Plano>();
    public DbSet<Matricula>            Matriculas           => Set<Matricula>();

    public AppDbContext(DbContextOptions<AppDbContext> opcoes) : base(opcoes) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Carrega todas as configurações do assembly de Infrastructure
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
