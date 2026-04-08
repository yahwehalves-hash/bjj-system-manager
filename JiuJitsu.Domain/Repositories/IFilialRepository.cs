using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface IFilialRepository
{
    Task AdicionarAsync(Filial filial, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Filial filial, CancellationToken cancellationToken = default);
    Task<Filial?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
