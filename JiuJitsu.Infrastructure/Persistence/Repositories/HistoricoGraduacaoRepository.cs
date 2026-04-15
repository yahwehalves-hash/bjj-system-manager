using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class HistoricoGraduacaoRepository : IHistoricoGraduacaoRepository
{
    private readonly AppDbContext _contexto;

    public HistoricoGraduacaoRepository(AppDbContext contexto) => _contexto = contexto;

    public async Task AdicionarAsync(HistoricoGraduacao historico, CancellationToken cancellationToken = default)
        => await _contexto.HistoricoGraduacoes.AddAsync(historico, cancellationToken);

    public async Task<HistoricoGraduacao?> ObterAtualAsync(Guid atletaId, CancellationToken cancellationToken = default)
        => await _contexto.HistoricoGraduacoes
            .Where(h => h.AtletaId == atletaId && h.DataFim == null)
            .OrderByDescending(h => h.DataInicio)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<HistoricoGraduacao>> ListarPorAtletaAsync(Guid atletaId, CancellationToken cancellationToken = default)
        => await _contexto.HistoricoGraduacoes
            .Where(h => h.AtletaId == atletaId)
            .OrderBy(h => h.DataInicio)
            .ToListAsync(cancellationToken);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await _contexto.SaveChangesAsync(cancellationToken);
}
