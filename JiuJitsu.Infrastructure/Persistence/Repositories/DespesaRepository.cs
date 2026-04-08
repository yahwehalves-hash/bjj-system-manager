using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class DespesaRepository : IDespesaRepository
{
    private readonly AppDbContext _contexto;

    public DespesaRepository(AppDbContext contexto) => _contexto = contexto;

    public async Task AdicionarAsync(Despesa despesa, CancellationToken cancellationToken = default)
        => await _contexto.Despesas.AddAsync(despesa, cancellationToken);

    public Task AtualizarAsync(Despesa despesa, CancellationToken cancellationToken = default)
    {
        _contexto.Despesas.Update(despesa);
        return Task.CompletedTask;
    }

    public async Task<Despesa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _contexto.Despesas.FindAsync([id], cancellationToken);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await _contexto.SaveChangesAsync(cancellationToken);
}
