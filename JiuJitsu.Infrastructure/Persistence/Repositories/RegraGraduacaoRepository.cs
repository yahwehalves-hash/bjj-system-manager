using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Enums;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class RegraGraduacaoRepository : IRegraGraduacaoRepository
{
    private readonly AppDbContext _db;

    public RegraGraduacaoRepository(AppDbContext db) => _db = db;

    public Task<RegraGraduacao?> ObterAsync(Guid? filialId, Faixa faixa, Grau grau, CancellationToken cancellationToken = default)
        => _db.RegrasGraduacao
            .FirstOrDefaultAsync(r => r.FilialId == filialId && r.Faixa == faixa && r.Grau == grau, cancellationToken);

    public async Task<IEnumerable<RegraGraduacao>> ListarAsync(Guid? filialId, CancellationToken cancellationToken = default)
        => await _db.RegrasGraduacao
            .Where(r => r.FilialId == filialId || r.FilialId == null)
            .ToListAsync(cancellationToken);

    public async Task AdicionarOuAtualizarAsync(RegraGraduacao regra, CancellationToken cancellationToken = default)
    {
        var existente = await _db.RegrasGraduacao.FindAsync([regra.Id], cancellationToken);
        if (existente is null)
            await _db.RegrasGraduacao.AddAsync(regra, cancellationToken);
        else
            _db.RegrasGraduacao.Update(regra);
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
