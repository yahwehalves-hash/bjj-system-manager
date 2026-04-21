using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class RegistroPresencaRepository : IRegistroPresencaRepository
{
    private readonly AppDbContext _db;

    public RegistroPresencaRepository(AppDbContext db) => _db = db;

    public async Task AdicionarAsync(RegistroPresenca registro, CancellationToken cancellationToken = default)
        => await _db.RegistrosPresenca.AddAsync(registro, cancellationToken);

    public async Task AdicionarEmLoteAsync(IEnumerable<RegistroPresenca> registros, CancellationToken cancellationToken = default)
        => await _db.RegistrosPresenca.AddRangeAsync(registros, cancellationToken);

    public Task<bool> JaRegistradoHojeAsync(Guid atletaId, Guid turmaId, CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.UtcNow.Date;
        return _db.RegistrosPresenca.AnyAsync(
            r => r.AtletaId == atletaId && r.TurmaId == turmaId && r.DataHora.Date == hoje,
            cancellationToken);
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
