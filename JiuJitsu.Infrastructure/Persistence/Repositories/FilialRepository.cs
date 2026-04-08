using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class FilialRepository : IFilialRepository
{
    private readonly AppDbContext _contexto;

    public FilialRepository(AppDbContext contexto) => _contexto = contexto;

    public async Task AdicionarAsync(Filial filial, CancellationToken cancellationToken = default)
        => await _contexto.Filiais.AddAsync(filial, cancellationToken);

    public Task AtualizarAsync(Filial filial, CancellationToken cancellationToken = default)
    {
        _contexto.Filiais.Update(filial);
        return Task.CompletedTask;
    }

    public async Task<Filial?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _contexto.Filiais.FindAsync([id], cancellationToken);

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
        => await _contexto.Filiais.AnyAsync(f => f.Id == id && f.Ativo, cancellationToken);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await _contexto.SaveChangesAsync(cancellationToken);
}
