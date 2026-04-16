using JiuJitsu.Domain.Entities;

namespace JiuJitsu.Domain.Repositories;

public interface IPlanoRepository
{
    Task<Plano?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Plano>> ListarAtivosAsync(Guid? filialId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Plano plano, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
