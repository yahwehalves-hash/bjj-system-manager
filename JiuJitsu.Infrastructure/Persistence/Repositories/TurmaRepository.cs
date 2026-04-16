using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class TurmaRepository : ITurmaRepository
{
    private readonly AppDbContext _db;

    public TurmaRepository(AppDbContext db) => _db = db;

    public async Task AdicionarAsync(Turma turma, CancellationToken cancellationToken = default)
        => await _db.Turmas.AddAsync(turma, cancellationToken);

    public Task AtualizarAsync(Turma turma, CancellationToken cancellationToken = default)
    {
        _db.Turmas.Update(turma);
        return Task.CompletedTask;
    }

    public Task<Turma?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Turmas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AdicionarAtletaAsync(AtletaTurma atletaTurma, CancellationToken cancellationToken = default)
        => await _db.AtletasTurmas.AddAsync(atletaTurma, cancellationToken);

    public async Task RemoverAtletaAsync(Guid atletaId, Guid turmaId, CancellationToken cancellationToken = default)
    {
        var vinculo = await _db.AtletasTurmas
            .FirstOrDefaultAsync(at => at.AtletaId == atletaId && at.TurmaId == turmaId, cancellationToken);
        if (vinculo is not null)
            _db.AtletasTurmas.Remove(vinculo);
    }

    public Task<bool> AtletaJaVinculadoAsync(Guid atletaId, Guid turmaId, CancellationToken cancellationToken = default)
        => _db.AtletasTurmas.AnyAsync(at => at.AtletaId == atletaId && at.TurmaId == turmaId, cancellationToken);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
