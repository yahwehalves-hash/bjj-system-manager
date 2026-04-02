using JiuJitsu.Domain.Entities;
using JiuJitsu.Domain.Repositories;
using JiuJitsu.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JiuJitsu.Infrastructure.Persistence.Repositories;

public class AtletaRepository : IAtletaRepository
{
    private readonly AppDbContext _contexto;

    public AtletaRepository(AppDbContext contexto) => _contexto = contexto;

    public async Task AdicionarAsync(Atleta atleta, CancellationToken cancellationToken = default)
        => await _contexto.Atletas.AddAsync(atleta, cancellationToken);

    public Task AtualizarAsync(Atleta atleta, CancellationToken cancellationToken = default)
    {
        _contexto.Atletas.Update(atleta);
        return Task.CompletedTask;
    }

    public async Task<Atleta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _contexto.Atletas.FindAsync([id], cancellationToken);

    public async Task<Atleta?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await _contexto.Atletas
            .FirstOrDefaultAsync(a => a.Cpf.Valor == cpf, cancellationToken);

    public async Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => await _contexto.Atletas
            .AnyAsync(a => a.Cpf.Valor == cpf, cancellationToken);

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _contexto.Atletas
            .AnyAsync(a => a.Email.Valor == email, cancellationToken);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await _contexto.SaveChangesAsync(cancellationToken);
}
