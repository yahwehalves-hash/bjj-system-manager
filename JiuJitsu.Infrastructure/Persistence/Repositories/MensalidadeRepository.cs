using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class MensalidadeRepository : IMensalidadeRepository
{
    private readonly AppDbContext _contexto;

    public MensalidadeRepository(AppDbContext contexto) => _contexto = contexto;

    public async Task AdicionarAsync(Mensalidade mensalidade, CancellationToken cancellationToken = default)
        => await _contexto.Mensalidades.AddAsync(mensalidade, cancellationToken);

    public async Task AdicionarVariasAsync(IEnumerable<Mensalidade> mensalidades, CancellationToken cancellationToken = default)
        => await _contexto.Mensalidades.AddRangeAsync(mensalidades, cancellationToken);

    public Task AtualizarAsync(Mensalidade mensalidade, CancellationToken cancellationToken = default)
    {
        _contexto.Mensalidades.Update(mensalidade);
        return Task.CompletedTask;
    }

    public async Task<Mensalidade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _contexto.Mensalidades.FindAsync([id], cancellationToken);

    public async Task<bool> ExisteParaAtletaNoMesAsync(Guid atletaId, DateOnly competencia, CancellationToken cancellationToken = default)
        => await _contexto.Mensalidades
            .AnyAsync(m => m.AtletaId == atletaId && m.Competencia == competencia, cancellationToken);

    public async Task<IEnumerable<Mensalidade>> ListarPendentesVencidasAsync(
        DateOnly dataReferencia,
        int toleranciaDias,
        CancellationToken cancellationToken = default)
    {
        var dataLimite = dataReferencia.AddDays(-toleranciaDias);
        return await _contexto.Mensalidades
            .Where(m => m.Status == Domain.Enums.StatusMensalidade.Pendente
                     && m.DataVencimento < dataLimite)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mensalidade>> ListarVencidasParaInadimplenciaAsync(
        DateOnly dataReferencia,
        int diasParaInadimplencia,
        CancellationToken cancellationToken = default)
    {
        var dataLimite = dataReferencia.AddDays(-diasParaInadimplencia);
        return await _contexto.Mensalidades
            .Where(m => m.Status == Domain.Enums.StatusMensalidade.Vencida
                     && m.DataVencimento < dataLimite)
            .ToListAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await _contexto.SaveChangesAsync(cancellationToken);
}
