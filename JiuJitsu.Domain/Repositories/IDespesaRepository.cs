using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface IDespesaRepository
{
    Task AdicionarAsync(Despesa despesa, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Despesa despesa, CancellationToken cancellationToken = default);
    Task<Despesa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
