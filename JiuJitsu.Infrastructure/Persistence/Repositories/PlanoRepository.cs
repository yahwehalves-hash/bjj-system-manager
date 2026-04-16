using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class PlanoRepository : IPlanoRepository
{
    private readonly AppDbContext _db;

    public PlanoRepository(AppDbContext db) => _db = db;

    public Task<Plano?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Planos.FindAsync([id], cancellationToken).AsTask();

    public async Task<IEnumerable<Plano>> ListarAtivosAsync(Guid? filialId, CancellationToken cancellationToken = default)
        => await _db.Planos
            .Where(p => p.Ativo && (p.FilialId == null || p.FilialId == filialId))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Plano plano, CancellationToken cancellationToken = default)
        => await _db.Planos.AddAsync(plano, cancellationToken);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
