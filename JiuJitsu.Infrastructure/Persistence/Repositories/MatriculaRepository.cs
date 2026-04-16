using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class MatriculaRepository : IMatriculaRepository
{
    private readonly AppDbContext _db;

    public MatriculaRepository(AppDbContext db) => _db = db;

    public Task<Matricula?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Matriculas
            .Include(m => m.Plano)
            .Include(m => m.Atleta)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<Matricula?> ObterAtivaDoAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default)
        => _db.Matriculas
            .Include(m => m.Plano)
            .FirstOrDefaultAsync(m => m.AtletaId == atletaId && m.Status == StatusMatricula.Ativa, cancellationToken);

    public async Task<IEnumerable<Matricula>> ListarPorAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default)
        => await _db.Matriculas
            .Include(m => m.Plano)
            .Include(m => m.Atleta)
            .Where(m => m.AtletaId == atletaId)
            .OrderByDescending(m => m.DataInicio)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Matricula matricula, CancellationToken cancellationToken = default)
        => await _db.Matriculas.AddAsync(matricula, cancellationToken);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
